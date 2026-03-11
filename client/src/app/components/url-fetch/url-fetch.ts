import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConversionService } from '../../services/conversion';
import { inject } from '@angular/core';

@Component({
  selector: 'app-url-fetch',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './url-fetch.html',
  styleUrl: './url-fetch.scss'
})
export class UrlFetch {
  direction = input<'json-to-csharp' | 'csharp-to-json'>('json-to-csharp');
  jsonFetched = output<string>();

  isOpen = signal(false);
  url = signal('');
  isLoading = signal(false);
  errorMessage = signal('');

  private conversionService = inject(ConversionService);

  toggle() {
    this.isOpen.set(!this.isOpen());
    if (!this.isOpen()) {
      this.errorMessage.set('');
    }
  }

  onUrlInput(value: string) {
    this.url.set(value);
    if (this.errorMessage()) this.errorMessage.set('');
  }

  fetch() {
    const url = this.url().trim();
    if (!url) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.conversionService.fetchJson(url).subscribe({
      next: result => {
        this.isLoading.set(false);
        this.jsonFetched.emit(result.json);
        this.isOpen.set(false);
        this.url.set('');
      },
      error: err => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.error ??
          err.error?.message ??
          'Could not fetch URL. Check the address and try again.'
        );
      }
    });
  }

  onKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter') this.fetch();
    if (event.key === 'Escape') this.isOpen.set(false);
  }
}