import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputEditor } from './input-editor';

describe('InputEditor', () => {
  let component: InputEditor;
  let fixture: ComponentFixture<InputEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputEditor],
    }).compileComponents();

    fixture = TestBed.createComponent(InputEditor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
