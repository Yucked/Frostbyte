<p align="center">
	<img src="https://imgur.com/wD16j6u.png" />
	</br>
	<p align="center">
	Rhapsody performs similar to Lavalink but implements a different design pattern. This design doc should aid you in writing your own client.<br>
	Before proceeding, please keep in mind this design is subject to change. It is your responsbibility to keep your clients up to date.
  </p>
</p>


---

<p align="center">
	<img src="https://imgur.com/Fkv4PLT.png" />
</p>

---
Rhapsody provides wide range of options to configure it to your liking.
```json
{
  "endpoint": {
    "host": "*",
    "port": 5000,
    "fallbackRandom": true
  },
  "logging": {
    "defaltLevel": "Trace",
    "filters": {
      "System.*": 0
    }
  },
  "authorization": {
    "password": "Rhapsody",
    "endpoints": [
      "/api/search",
      "/ws/{guildId}"
    ]
  },
  "sourceProviders": {
    "youtube": true,
    "soundcloud": true
  }
}
```

- `host:` Can be set to `*` to listen on both IPV4 and IPV6 addresses from any interface.
- `fallbackRandom`: Used if a port is unavailable or if an address is unavailable.
- `logging.defaltLevel`: The following log levels are supported: None, Trace, Debug, Information, Warning, Error, Critical.
- `logging.filters:` Don't like receiving too many messages from `System.Threading.Tasks`? or anything from `System` namespace?
You can specify a log filter for namespaces. If you'd like to block anything from `System.XYZ` use `System.*` and everything will fall under `System` instead.
- `authorization.endpoints`: You can specify which routes require authorization.
- `sourceProviders`: You can toggle source providers.

Now that you have a basic understanding of how you can configure Rhapsody let's discuss routes.

<p align="center">
	<img src="https://imgur.com/8M8e0Lv.png" />
</p>

---
All REST routes begin with `{endpoint}/api/` these routes can be be found in `Controllers` directory.\
Here are the following routes that are available:
- `/ping`: Returns datetime for now
- `/player/{guildId}`: Let's you configure your guild player. This is where payloads are sent.
- `/search/{provider}`: Depends on which source provider you have enabled in options.
- `/ws/{guildId}`: This is where you establish a websocket connection for your guild to receive track and stat updates.
 
 Additionally, `/player/{guildId}` handles few CRUD operations such as GET, DELETE, POST.
 - `GET`: Returns guild player information.
 - `DELETE`: To remove player after your Discord client has lost connection or just want to remove player.
 - `POST`: Send payloads related to your guild player. 
 
 <p align="center">
 	<img src="https://imgur.com/wMorVUM.png" />
 </p>
 
 ---
 Without a `ConnectPayload` you can't establish any WS connection or perform player related REST operations.
 The very first payload sent should be `Connect`.
 
 ```json5
{
  "endpoint": "...",  // Voice server endpoint
  "token": "...",     // Voice server token
  "sessionId": "...", // Voice server session id
  "userId": 123456789012345 // Your discord client id
}
```