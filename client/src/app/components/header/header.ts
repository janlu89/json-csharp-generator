import { Component, inject } from '@angular/core';
import { ThemeService } from  '../../services/theme';

@Component({
  selector: 'app-header',
  standalone: true,
  template: `
    <header class="header">
      <div class="header-left">
        <span class="logo">JSON <span class="accent">↔</span> C#</span>
        <span class="tagline">Paste JSON, get C# models. Or the other way around.</span>
      </div>
      <div class="header-right">
        <button
          class="theme-toggle"
          (click)="themeService.toggle()"
          [attr.aria-label]="themeService.isDark() ? 'Switch to light theme' : 'Switch to dark theme'">
          {{ themeService.isDark() ? '☀' : '☾' }}
        </button>
      </div>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: var(--header-height);
      padding: 0 var(--space-lg);
      background-color: var(--bg-secondary);
      border-bottom: 1px solid var(--border);
      flex-shrink: 0;
    }

    .header-left {
      display: flex;
      align-items: baseline;
      gap: var(--space-md);
    }

    .logo {
      font-size: var(--font-size-xl);
      font-weight: 600;
      color: var(--text-primary);
      letter-spacing: -0.5px;
    }

    .accent {
      color: var(--accent);
    }

    .tagline {
      font-size: var(--font-size-sm);
      color: var(--text-secondary);
    }

    .theme-toggle {
      background: none;
      border: 1px solid var(--border);
      color: var(--text-secondary);
      border-radius: var(--border-radius);
      width: 32px;
      height: 32px;
      cursor: pointer;
      font-size: var(--font-size-base);
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--transition);

      &:hover {
        border-color: var(--accent);
        color: var(--accent);
      }
    }
  `]
})
export class Header {
  themeService = inject(ThemeService);
}