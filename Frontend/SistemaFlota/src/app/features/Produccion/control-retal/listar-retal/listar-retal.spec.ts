import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarRetal } from './listar-retal';

describe('ListarRetal', () => {
  let component: ListarRetal;
  let fixture: ComponentFixture<ListarRetal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarRetal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarRetal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
