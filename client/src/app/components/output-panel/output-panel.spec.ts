import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OutputPanel } from './output-panel';

describe('OutputPanel', () => {
  let component: OutputPanel;
  let fixture: ComponentFixture<OutputPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OutputPanel],
    }).compileComponents();

    fixture = TestBed.createComponent(OutputPanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
