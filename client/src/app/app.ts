import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { Header } from './components/header/header';
import { DirectionToggle, ConversionDirection } from './components/direction-toggle/direction-toggle';
import { SplitPane } from './components/split-pane/split-pane';
import { UrlFetch } from './components/url-fetch/url-fetch';
import { ThemeService } from './services/theme';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Header, DirectionToggle, SplitPane, UrlFetch],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  @ViewChild(SplitPane) splitPane!: SplitPane;

  private themeService = inject(ThemeService);
  direction = signal<ConversionDirection>('json-to-csharp');

  ngOnInit() {
    this.themeService.init();
  }

  onDirectionChange(dir: ConversionDirection) {
    this.direction.set(dir);
  }

  onJsonFetched(json: string) {
  this.splitPane.outputValue.set('');
  this.splitPane.errorMessage.set('');
  this.splitPane.inputEditor.setValue(json);
}
}