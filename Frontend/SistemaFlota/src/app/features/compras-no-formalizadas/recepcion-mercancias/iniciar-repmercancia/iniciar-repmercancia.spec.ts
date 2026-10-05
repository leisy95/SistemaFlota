import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IniciarRepmercancia } from './iniciar-repmercancia';

describe('IniciarRepmercancia', () => {
  let component: IniciarRepmercancia;
  let fixture: ComponentFixture<IniciarRepmercancia>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IniciarRepmercancia]
    })
    .compileComponents();

    fixture = TestBed.createComponent(IniciarRepmercancia);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
