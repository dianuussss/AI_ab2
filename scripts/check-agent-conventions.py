#!/usr/bin/env python3
"""Check architecture conventions for the Lab1 library project."""

from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from pathlib import Path


SRC_PARTS = ("src", "Lab1")
USING_RE = re.compile(r"^using\s+([^;]+);", re.MULTILINE)
SERVICE_METHOD_RE = re.compile(r"\s{4}([A-Za-z0-9_<>?]+)\s+([A-Za-z0-9_]+)\(")
TEST_METHOD_RE = re.compile(r"public\s+void\s+([A-Za-z0-9_]+)\(")


@dataclass(frozen=True)
class Violation:
    code: str
    path: Path
    line: int
    message: str


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", default=".", help="Repository root")
    args = parser.parse_args()

    root = Path(args.root).resolve()
    violations = collect_violations(root)

    print(f"Checked root: {root}")
    print(f"Violations: {len(violations)}")

    for violation in violations:
        relative = violation.path.relative_to(root)
        print(f"{violation.code}: {relative}:{violation.line}: {violation.message}")

    return 1 if violations else 0


def collect_violations(root: Path) -> list[Violation]:
    source_files = [
        path
        for path in root.rglob("*.cs")
        if is_project_source(path) and "bin" not in path.parts and "obj" not in path.parts
    ]

    violations: list[Violation] = []
    for path in source_files:
        text = path.read_text(encoding="utf-8")
        layer = detect_layer(path)
        violations.extend(check_file(path, text, layer))

    violations.extend(check_service_tests(root))
    return sorted(violations, key=lambda item: (str(item.path), item.line, item.code))


def is_project_source(path: Path) -> bool:
    parts = path.parts
    return all(part in parts for part in SRC_PARTS) or "tests" in parts


def detect_layer(path: Path) -> str:
    parts = path.parts
    if "Itmo.ObjectOrientedProgramming.Lab1.Domain" in parts:
        return "domain"
    if "Itmo.ObjectOrientedProgramming.Lab1.Application" in parts:
        return "application"
    if "Itmo.ObjectOrientedProgramming.Lab1.Presentation" in parts:
        return "presentation"
    if "Itmo.ObjectOrientedProgramming.Lab1.Infrastructure" in parts:
        return "infrastructure"
    return "other"


def check_file(path: Path, text: str, layer: str) -> list[Violation]:
    violations: list[Violation] = []

    if "NotImplementedException" in text:
        violations.append(
            violation("SRC001", path, text, "NotImplementedException", "committed source must be implemented")
        )

    for namespace in USING_RE.findall(text):
        if layer == "domain" and contains_any(namespace, ".Application", ".Infrastructure", ".Presentation"):
            violations.append(
                violation("ARCH001", path, text, namespace, "Domain must not depend on outer layers")
            )

        if layer == "application" and contains_any(namespace, ".Presentation", ".Infrastructure.InMemory"):
            violations.append(
                violation("ARCH002", path, text, namespace, "Application must not depend on presentation or concrete infrastructure")
            )

        if layer == "presentation" and ".Infrastructure" in namespace:
            violations.append(
                violation("ARCH003", path, text, namespace, "Presentation must depend on ILibraryService, not repositories")
            )

    if layer == "presentation" and "Controllers" in path.parts:
        if "IUserRepository" in text or "IBookRepository" in text or "IRequestRepository" in text:
            violations.append(
                violation("ARCH004", path, text, "Repository", "Controllers must not use repositories directly")
            )

        if "BusinessRuleViolationException" in text:
            violations.append(
                violation("ERR001", path, text, "BusinessRuleViolationException", "Controllers should not own business-rule failures")
            )

    if layer == "application" and "LibraryService.cs" in path.name:
        if "new InMemory" in text:
            violations.append(
                violation("ARCH005", path, text, "new InMemory", "Services must receive repositories through abstractions")
            )

        if "InvalidOperationException" in text:
            violations.append(
                violation("ERR002", path, text, "InvalidOperationException", "Use BusinessRuleViolationException for expected business failures")
            )

    if layer == "domain" and "throw new BusinessRuleViolationException" in text:
        violations.append(
            violation("ERR003", path, text, "BusinessRuleViolationException", "Domain models should keep state rules local and not import application exceptions")
        )

    return violations


def check_service_tests(root: Path) -> list[Violation]:
    service_interface = root / "src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Application/Abstractions/ILibraryService.cs"
    tests_dir = root / "tests/Lab1.Tests"

    if not service_interface.exists() or not tests_dir.exists():
        return []

    interface_text = service_interface.read_text(encoding="utf-8")
    method_names = {
        match.group(2)
        for match in SERVICE_METHOD_RE.finditer(interface_text)
        if match.group(2) != "ILibraryService"
    }

    tests_text = "\n".join(path.read_text(encoding="utf-8") for path in tests_dir.rglob("*.cs"))
    test_names = TEST_METHOD_RE.findall(tests_text)

    violations: list[Violation] = []
    for method_name in sorted(method_names):
        if not any(method_name in test_name for test_name in test_names):
            violations.append(
                Violation(
                    "TEST001",
                    service_interface,
                    find_line(interface_text, method_name),
                    f"Public service method {method_name} must be covered by a named test",
                )
            )

    return violations


def contains_any(value: str, *needles: str) -> bool:
    return any(needle in value for needle in needles)


def violation(code: str, path: Path, text: str, needle: str, message: str) -> Violation:
    return Violation(code, path, find_line(text, needle), message)


def find_line(text: str, needle: str) -> int:
    for index, line in enumerate(text.splitlines(), start=1):
        if needle in line:
            return index

    return 1


if __name__ == "__main__":
    raise SystemExit(main())
