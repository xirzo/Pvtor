## Telegram bot 

I wanna implement webhook non-blocking solution, sources:

- https://habr.com/ru/companies/otus/articles/786754/
- https://github.com/TelegramBots/Telegram.Bot
- https://itproblog.ru/%D0%BD%D0%B0%D1%81%D1%82%D1%80%D0%BE%D0%B9%D0%BA%D0%B0-sftp-%D1%81%D0%B5%D1%80%D0%B2%D0%B5%D1%80%D0%B0-%D0%BD%D0%B0-linux/

### Run CI/CD locally

Create a *.env* file at *.secrets* directory put all of the secrets there, here is an example:

```
DB_CONNECTION_STRING=Data Source=Pvtor.db;
TELEGRAM_BOT_TOKEN=TOKEN
TELEGRAM_BOT_WEBHOOK_URL=https://telegram.example.com/api/telegram
TELEGRAM_SECRET_TOKEN=SECRET_TOKEN
SFTP_SERVER=sftp.example.com
SFTP_USERNAME=deploy_user
SFTP_PASSWORD=deploy_password
```

```sh
act --secret-file .secrets/.env
```
