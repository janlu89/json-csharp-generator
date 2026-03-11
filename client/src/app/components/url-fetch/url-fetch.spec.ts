import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UrlFetch } from './url-fetch';

describe('UrlFetch', () => {
  let component: UrlFetch;
  let fixture: ComponentFixture<UrlFetch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UrlFetch],
    }).compileComponents();

    fixture = TestBed.createComponent(UrlFetch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
