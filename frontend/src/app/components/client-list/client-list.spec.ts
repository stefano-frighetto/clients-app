import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClientList } from './client-list';
import { ClientService } from '../../services/client';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { ChangeDetectorRef } from '@angular/core';
import { vi } from 'vitest';

describe('ClientList', () => {
  let component: ClientList;
  let fixture: ComponentFixture<ClientList>;
  
  const clientServiceMock = {
    getAll: vi.fn().mockReturnValue(of([])),
    delete: vi.fn()
  };

  const dialogMock = {
    open: vi.fn().mockReturnValue({ afterClosed: () => of(null) })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClientList, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        { provide: ClientService, useValue: clientServiceMock },
        { provide: MatDialog, useValue: dialogMock },
        ChangeDetectorRef
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClientList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load clients on init', () => {
    expect(clientServiceMock.getAll).toHaveBeenCalled();
  });

  it('should open dialog when createClient is called', () => {
    component.createClient();
    expect(dialogMock.open).toHaveBeenCalled();
  });
});