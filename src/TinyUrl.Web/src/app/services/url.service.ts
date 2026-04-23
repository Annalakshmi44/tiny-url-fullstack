import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { ShortUrl, CreateUrlRequest, ApiResponse } from '../models/url.model';

@Injectable({
  providedIn: 'root'
})
export class UrlService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/urls`;

  createShortUrl(request: CreateUrlRequest): Observable<ShortUrl> {
    return this.http.post<ApiResponse<ShortUrl>>(this.apiUrl, request)
      .pipe(map(response => response.data!));
  }

  getPublicUrls(search?: string): Observable<ShortUrl[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<ApiResponse<ShortUrl[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data ?? []));
  }

  deleteUrl(shortCode: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${shortCode}`);
  }
}
