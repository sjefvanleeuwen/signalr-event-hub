# SIGNALR Event Hub
Generic event messaging hub mainly for front end signalling if an asycnhronous task has completed in the backend. This runs via websockets. There are 3 websocket node types.

The main goal of this setup is to update the User Interface when tasks categorized in topics complete their work. Also live chat can be supported with this setup.

![signalr-setup](./doc/signalr-setup.png)

1. The SIGNALR Server contains a generic hub for multicasting topics to connected client.
2. The SIGNALR Client Console publishes events on topics
3. The SIGNALR Client Angular UI front end is the subscriber to these topic

## Status

The project is under construction. The nodes currently show simple event messaging and CORS is currently setup.

## Run this sample

Open three Terminals in VSCODE to run the three nodes.

```
~\signalr-event-hub\src\signalr-event-hubt>dotnet run
~\signalr-event-hub\src\signalr-test-client>dotnet run
~\signalr-event-hub\src\signalr-test-angular>dotnet run

```

Browse to http://localhost:5050 

And choose the SignalR menu. You will see messages arriving from the client console.

![signalr-setup](./doc/firefox.signalr.png)

### Angular Component (code listing)
```typescript
import { Component, OnInit } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Component({
  selector: 'app-signalr-topic',
  templateUrl: './signalr-topic.component.html',
  styleUrls: ['./signalr-topic.component.css']
})
export class SignalRTopicComponent implements OnInit {
  public hubConnection: HubConnection;

  ngOnInit(): void {
    const divMessages = document.querySelector("#divMessages");
    this.hubConnection = new HubConnectionBuilder()
      .withUrl("http://localhost:5051/eventhub")
      .build();
    this.hubConnection.start().catch(err => document.write(err));
    this.hubConnection.on("echo", (message: string) => {
      let m = document.createElement("div");
      m.innerHTML =
          '<div class="message__author">' + message + '</div>';
      divMessages.appendChild(m);
    });
  }
}
```

## Built With

* [VSCODE](https://code.visualstudio.com/) - The IDE used
* [DOCKER](https://www.docker.com/) - Build, Ship, and Run Any App, Anywhere

## Contributing

Pull requests are accepted

## Authors

* **Sjef van Leeuwen** - *Initial work* - [github](https://github.com/sjefvanleeuwen)

## License

This project is licensed under the GPL-V3 License - see the [LICENSE.md](LICENSE.md) file for details
