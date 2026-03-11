import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SplitPane } from './split-pane';

describe('SplitPane', () => {
  let component: SplitPane;
  let fixture: ComponentFixture<SplitPane>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SplitPane],
    }).compileComponents();

    fixture = TestBed.createComponent(SplitPane);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
