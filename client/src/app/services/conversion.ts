import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment';

export interface GenerationOptions {
  rootClassName: string;
  useNullableReferenceTypes: boolean;
  namingConvention: 'PascalCase' | 'CamelCase';
  attributeStyle: 'None' | 'SystemTextJson' | 'Newtonsoft';
  generateAsRecord: boolean;
  usePreciseTypes: boolean;
  arrayItemClassName: string;
}

export interface ConversionResult {
  success: boolean;
  output?: string;
  errorMessage?: string;
}

export const DEFAULT_OPTIONS: GenerationOptions = {
  rootClassName: 'Root',
  useNullableReferenceTypes: true,
  namingConvention: 'PascalCase',
  attributeStyle: 'SystemTextJson',
  generateAsRecord: false,
  usePreciseTypes: false,
  arrayItemClassName: 'Item'
};

@Injectable({ providedIn: 'root' })
export class ConversionService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  jsonToCsharp(json: string, options: GenerationOptions): Observable<ConversionResult> {
    return this.http.post<ConversionResult>(`${this.baseUrl}/api/json-to-csharp`, {
      json,
      options
    });
  }

  csharpToJson(csharpCode: string): Observable<ConversionResult> {
    return this.http.post<ConversionResult>(`${this.baseUrl}/api/csharp-to-json`, {
      csharpCode
    });
  }

  fetchJson(url: string): Observable<{ json: string }> {
    return this.http.post<{ json: string }>(`${this.baseUrl}/api/fetch-json`, {
      url
    });
  }
}