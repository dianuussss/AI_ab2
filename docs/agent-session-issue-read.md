# Экспорт сессии: промпт без текста issue и вызов чтения

Источник: транскрипт Cursor `157ccf1e-6a2e-4f26-9e3d-81b9b2c5d61f`, 17 октября 2026, 20:12 (UTC+3).

Ниже только сообщение пользователя на шаг реализации и вызовы чтения issue. Тело issue в экспорт не входит: в промпте его не было, а ответ API здесь не нужен.

## Промпт

```text
https://github.com/dianuussss/AI_ab2/issues/1
```

В сообщении нет business context, acceptance criteria, edge cases и скрытой инструкции. Есть только ссылка на issue.

## Вызов чтения issue

Сначала агент вызвал `gh`:

```bash
gh issue view 1 --repo dianuussss/AI_ab2
```

`gh` в той среде не увидел keyring. Следующий вызов — чтение того же issue через GitHub API:

```text
WebFetch
url: https://api.github.com/repos/dianuussss/AI_ab2/issues/1
```

После этого вызова агент перешёл к коду (`CancelRequest` / `Cancelled`), тестам и PR. Текст постановки в промпт не подставлялся.
