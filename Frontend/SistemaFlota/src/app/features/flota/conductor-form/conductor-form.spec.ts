import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConductorForm } from './conductor-form';

describe('ConductorForm', () => {
  let component: ConductorForm;
  let fixture: ComponentFixture<ConductorForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConductorForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConductorForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
