import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClienteBodega } from './cliente-bodega';

describe('ClienteBodega', () => {
  let component: ClienteBodega;
  let fixture: ComponentFixture<ClienteBodega>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClienteBodega]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClienteBodega);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
