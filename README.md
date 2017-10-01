# Задание

Напишите сервер статистики для многопользовательской игры-шутера. Матчи этой игры проходят на разных серверах, и задача сервера статистики — строить общую картину по результатам матчей со всех серверов.
Сервер должен представлять собой standalone-приложение, реализующее описанный ниже RESTful API.

Общая схема работы такая: игровые сервера анонсируют себя advertise-запросами, затем присылают результаты каждого завершенного матча. Сервер статистики аккумулирует разную статистику по результатам матчей и отдает её по запросам (статистика по серверу, статистика по игроку, топ игроков и т.д.).

API сервера статистики состоит из следующих методов:

- /servers/<endpoint>/info PUT, GET
- /servers/<endpoint>/matches/<timestamp> PUT, GET

- /servers/info GET

- /servers/<endpoint>/stats GET
- /players/<name>/stats GET

- /reports/recent-matches/\<count\> GET
- /reports/best-players/\<count\> GET
- /reports/popular-servers[/\<count\>] GET



Пример ответа на запрос 
GET /reports/recent-matches[\<count\>]
```
[
	{
		"server": "62.210.26.88-1337",
		"timestamp": "2017-01-22T15:11:12Z",
		"results":	{
	"map": "DM-HelloWorld",
	"gameMode": "DM",
	"fragLimit": 20,
	"timeLimit": 20,
	"timeElapsed": 12.345678,
	"scoreboard": [
		{
			"name": "Player1",
			"frags": 20,
			"kills": 21,
			"deaths": 3
		},
		{
			"name": "Player2",
			"frags": 2,
			"kills": 2,
			"deaths": 21
		}
]
}
	},
	...
]
```
