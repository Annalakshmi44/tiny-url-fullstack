import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { UrlFormComponent } from './components/url-form/url-form.component';
import { UrlListComponent } from './components/url-list/url-list.component';
import { ShortUrl } from './models/url.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, HttpClientModule, UrlFormComponent, UrlListComponent],
  template: `
    <div class="container">
      <header>
        <h1>Tiny URL</h1>
      </header>
      <main>
        <app-url-form (urlCreated)="onUrlCreated($event)"></app-url-form>
        <app-url-list #urlList></app-url-list>
      </main>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: #f5f5f5;
    }
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }
    header {
      text-align: center;
      margin-bottom: 2rem;
    }
    h1 {
      font-size: 2.5rem;
      color: #333;
      margin: 0;
    }
  `]
})
export class AppComponent {
  @ViewChild('urlList') urlList!: UrlListComponent;

  onUrlCreated(url: ShortUrl): void {
    this.urlList.refreshList();
  }
}
