import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ClientService } from './client';
import { environment } from '../../environments/environment';
import { Client, CreateClientDto } from '../models/client.model';

describe('ClientService', () => {
  let service: ClientService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ClientService
      ]
    });
    service = TestBed.inject(ClientService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should retrieve all clients (GET)', () => {
    const dummyClients: Client[] = [
      { clientId: 1, firstName: 'John', lastName: 'Doe', email: 'john@test.com', corporateName: 'A', cuit: '20-111', birthdate: '2000-01-01', cellPhone: '111' }
    ];

    service.getAll().subscribe(clients => {
      expect(clients.length).toBe(1);
      expect(clients).toEqual(dummyClients);
    });

    const req = httpMock.expectOne(environment.apiUrl);
    expect(req.request.method).toBe('GET');
    req.flush(dummyClients);
  });

  it('should create a client (POST)', () => {
    const newClient: CreateClientDto = {
      firstName: 'Jane', lastName: 'Doe', email: 'jane@test.com', corporateName: 'B', cuit: '27-222', birthdate: '1990-01-01', cellPhone: '222'
    };

    service.create(newClient).subscribe(client => {
      expect(client.firstName).toBe('Jane');
    });

    const req = httpMock.expectOne(environment.apiUrl);
    expect(req.request.method).toBe('POST');
    req.flush({ clientId: 2, ...newClient });
  });

  it('should search clients (GET with params)', () => {
    service.search('Juan').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/search?name=Juan`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});