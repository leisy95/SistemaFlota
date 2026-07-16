import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FadeSlideAnimation } from './fade-slide.animation';

describe('FadeSlideAnimation', () => {
  let component: FadeSlideAnimation;
  let fixture: ComponentFixture<FadeSlideAnimation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FadeSlideAnimation]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FadeSlideAnimation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
