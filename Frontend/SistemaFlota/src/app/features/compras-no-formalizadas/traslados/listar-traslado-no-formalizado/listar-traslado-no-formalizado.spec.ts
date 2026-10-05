import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarTrasladoNoFormalizado } from './listar-traslado-no-formalizado';

describe('ListarTrasladoNoFormalizado', () => {
  let component: ListarTrasladoNoFormalizado;
  let fixture: ComponentFixture<ListarTrasladoNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarTrasladoNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarTrasladoNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
