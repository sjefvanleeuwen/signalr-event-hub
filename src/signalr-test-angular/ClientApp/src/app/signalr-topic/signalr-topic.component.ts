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
      .withUrl("http://localhost:5050/eventhub")
      .build();
    this.hubConnection.start().catch(err => document.write(err));
    this.hubConnection.on("echo", (message: string) => {
      let m = document.createElement("div");
      m.innerHTML =
          '<div class="message__author">' + message + '</div>';
      divMessages.appendChild(m);
      //divMessages.scrollTop = divMessages.scrollHeight;
    });
  }
}
