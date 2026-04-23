export interface ShortUrl {
  id: number;
  shortCode: string;
  shortUrl: string;
  originalUrl: string;
  isPrivate: boolean;
  clickCount: number;
  createdAt: Date;
}

export interface CreateUrlRequest {
  url: string;
  isPrivate: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}
