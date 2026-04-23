import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UrlService } from '../../services/url.service';
import { ClipboardService } from '../../services/clipboard.service';
import { ShortUrl } from '../../models/url.model';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
  selector: 'app-url-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="url-list">
      <h2>Public URLs</h2>
      <div class="search-box">
        <input
          type="text"
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearch($event)"
          placeholder="Search URLs..."
        />
      </div>

      @if (isLoading) {
        <p class="loading">Loading...</p>
      } @else if (urls.length === 0) {
        <p class="empty">No public URLs found.</p>
      } @else {
        <div class="urls">
          @for (url of urls; track url.id) {
            <div class="url-card">
              <div class="url-info">
                <a [href]="url.shortUrl" target="_blank" class="short-url">
                  {{ url.shortUrl }}
                </a>
                <span class="badge">{{ url.clickCount }} clicks</span>
              </div>
              <p class="original-url">{{ url.originalUrl }}</p>
              <div class="actions">
                <button class="copy-btn" (click)="copyUrl(url)" [class.copied]="copiedId === url.id">
                  {{ copiedId === url.id ? 'Copied!' : 'Copy' }}
                </button>
                <button class="delete-btn" (click)="deleteUrl(url.shortCode)">Delete</button>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .url-list {
      max-width: 800px;
      margin: 0 auto;
      padding: 1.5rem;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    h2 { margin: 0 0 1rem; color: #333; }
    .search-box {
      margin-bottom: 1rem;
    }
    .search-box input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }
    .loading, .empty {
      text-align: center;
      color: #666;
      padding: 2rem;
    }
    .urls { display: flex; flex-direction: column; gap: 1rem; }
    .url-card {
      padding: 1rem;
      border: 1px solid #eee;
      border-radius: 6px;
      transition: box-shadow 0.2s;
    }
    .url-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
    .url-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.5rem;
    }
    .short-url {
      color: #0066cc;
      font-weight: 500;
      text-decoration: none;
    }
    .short-url:hover { text-decoration: underline; }
    .badge {
      background: #28a745;
      color: white;
      padding: 0.25rem 0.5rem;
      border-radius: 12px;
      font-size: 0.75rem;
    }
    .original-url {
      color: #666;
      font-size: 0.875rem;
      margin: 0 0 0.75rem;
      word-break: break-all;
    }
    .actions { display: flex; gap: 0.5rem; }
    button {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.875rem;
      transition: all 0.2s;
    }
    .copy-btn {
      background: #007bff;
      color: white;
    }
    .copy-btn:hover { background: #0056b3; }
    .copy-btn.copied { background: #28a745; }
    .delete-btn {
      background: #dc3545;
      color: white;
    }
    .delete-btn:hover { background: #c82333; }
  `]
})
export class UrlListComponent implements OnInit {
  private readonly urlService = inject(UrlService);
  private readonly clipboardService = inject(ClipboardService);
  private readonly searchSubject = new Subject<string>();

  urls: ShortUrl[] = [];
  searchTerm = '';
  isLoading = false;
  copiedId: number | null = null;

  ngOnInit(): void {
    this.loadUrls();
    
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        this.isLoading = true;
        return this.urlService.getPublicUrls(term);
      })
    ).subscribe({
      next: (urls) => {
        this.urls = urls;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  loadUrls(): void {
    this.isLoading = true;
    this.urlService.getPublicUrls().subscribe({
      next: (urls) => {
        this.urls = urls;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  async copyUrl(url: ShortUrl): Promise<void> {
    const success = await this.clipboardService.copy(url.shortUrl);
    if (success) {
      this.copiedId = url.id;
      setTimeout(() => this.copiedId = null, 2000);
    }
  }

  deleteUrl(shortCode: string): void {
    if (!confirm('Are you sure you want to delete this URL?')) return;
    
    this.urlService.deleteUrl(shortCode).subscribe({
      next: () => {
        this.urls = this.urls.filter(u => u.shortCode !== shortCode);
      }
    });
  }

  refreshList(): void {
    this.loadUrls();
  }
}
