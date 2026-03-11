import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DirectionToggle } from './direction-toggle';

describe('DirectionToggle', () => {
  let component: DirectionToggle;
  let fixture: ComponentFixture<DirectionToggle>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectionToggle],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectionToggle);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
