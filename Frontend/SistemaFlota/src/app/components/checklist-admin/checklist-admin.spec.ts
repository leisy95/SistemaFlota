import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChecklistAdmin } from './checklist-admin';

describe('ChecklistAdmin', () => {
  let component: ChecklistAdmin;
  let fixture: ComponentFixture<ChecklistAdmin>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChecklistAdmin]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChecklistAdmin);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
