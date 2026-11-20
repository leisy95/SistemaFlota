import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarCrispetas } from './listar-crispetas';

describe('ListarCrispetas', () => {
  let component: ListarCrispetas;
  let fixture: ComponentFixture<ListarCrispetas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarCrispetas]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarCrispetas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
