import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly STORAGE_KEY = 'json-csharp-theme';

  readonly isDark = signal<boolean>(true);

  init() {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    const prefersDark = stored !== null ? stored === 'dark' : true;
    this.isDark.set(prefersDark);
    this.applyTheme(prefersDark);
  }

  toggle() {
    const newValue = !this.isDark();
    this.isDark.set(newValue);
    this.applyTheme(newValue);
    localStorage.setItem(this.STORAGE_KEY, newValue ? 'dark' : 'light');
  }

  private applyTheme(isDark: boolean) {
    document.body.classList.toggle('light-theme', !isDark);
  }
}