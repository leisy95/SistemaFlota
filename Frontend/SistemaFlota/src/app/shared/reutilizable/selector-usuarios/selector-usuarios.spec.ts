import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectorUsuarios } from './selector-usuarios';

describe('SelectorUsuarios', () => {
  let component: SelectorUsuarios;
  let fixture: ComponentFixture<SelectorUsuarios>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectorUsuarios]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectorUsuarios);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
