import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UrlService } from '../../services/url.service';
import { ShortUrl } from '../../models/url.model';

@Component({
  selector: 'app-url-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="url-form">
      <h2>Shorten a URL</h2>
      <form (ngSubmit)="onSubmit()" #form="ngForm">
        <div class="input-group">
          <input
            type="url"
            [(ngModel)]="url"
            name="url"
            placeholder="Enter URL to shorten"
            required
            [disabled]="isLoading"
          />
          <label class="private-checkbox">
            <input type="checkbox" [(ngModel)]="isPrivate" name="isPrivate" />
            Private
          </label>
        </div>
        <button type="submit" [disabled]="!url || isLoading">
          {{ isLoading ? 'Generating...' : 'Generate' }}
        </button>
      </form>
      @if (error) {
        <p class="error">{{ error }}</p>
      }
    </div>
  `,
  styles: [`
    .url-form {
      max-width: 600px;
      margin: 0 auto 2rem;
      padding: 1.5rem;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    h2 { margin: 0 0 1rem; color: #333; }
    .input-group {
      display: flex;
      gap: 1rem;
      margin-bottom: 1rem;
      align-items: center;
    }
    input[type="url"] {
      flex: 1;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }
    .private-checkbox {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
      white-space: nowrap;
    }
    button {
      width: 100%;
      padding: 0.75rem;
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    button:hover:not(:disabled) { background: #c82333; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    .error { color: #dc3545; margin-top: 0.5rem; }
  `]
})
export class UrlFormComponent {
  @Output() urlCreated = new EventEmitter<ShortUrl>();

  private readonly urlService = inject(UrlService);

  url = '';
  isPrivate = false;
  isLoading = false;
  error = '';

  onSubmit(): void {
    if (!this.url) return;

    this.isLoading = true;
    this.error = '';

    this.urlService.createShortUrl({ url: this.url, isPrivate: this.isPrivate })
      .subscribe({
        next: (result) => {
          this.urlCreated.emit(result);
          this.url = '';
          this.isPrivate = false;
          this.isLoading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to create short URL';
          this.isLoading = false;
        }
      });
  }
}
