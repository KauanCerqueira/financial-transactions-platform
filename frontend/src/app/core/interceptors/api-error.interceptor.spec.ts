import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiError } from '../models/api-error';
import { apiErrorInterceptor } from './api-error.interceptor';

describe('apiErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([apiErrorInterceptor])), provideHttpClientTesting()],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function failingRequest(): Promise<ApiError> {
    return new Promise((resolve) => {
      http.get('/api/accounts').subscribe({ error: (error: ApiError) => resolve(error) });
    });
  }

  it('translates INSUFFICIENT_FUNDS into a business message', async () => {
    const result = failingRequest();
    httpMock
      .expectOne('/api/accounts')
      .flush({ code: 'INSUFFICIENT_FUNDS' }, { status: 422, statusText: 'Unprocessable' });

    const error = await result;

    expect(error).toBeInstanceOf(ApiError);
    expect(error.code).toBe('INSUFFICIENT_FUNDS');
    expect(error.message).toContain('Saldo insuficiente');
  });

  it('translates DUPLICATE_EVENT into a business message', async () => {
    const result = failingRequest();
    httpMock.expectOne('/api/accounts').flush({ code: 'DUPLICATE_EVENT' }, { status: 409, statusText: 'Conflict' });

    const error = await result;

    expect(error.code).toBe('DUPLICATE_EVENT');
    expect(error.message).toContain('já foi processado');
  });

  it('translates a network failure', async () => {
    const result = failingRequest();
    httpMock.expectOne('/api/accounts').error(new ProgressEvent('error'));

    const error = await result;

    expect(error.code).toBe('NETWORK_ERROR');
    expect(error.status).toBe(0);
  });

  it('hides technical detail on server errors', async () => {
    const result = failingRequest();
    httpMock.expectOne('/api/accounts').flush(null, { status: 500, statusText: 'Server Error' });

    const error = await result;

    expect(error.code).toBe('HTTP_500');
    expect(error.message).toContain('erro inesperado');
  });

  it('keeps the detail sent for validation errors', async () => {
    const result = failingRequest();
    httpMock
      .expectOne('/api/accounts')
      .flush({ detail: 'Payload inválido.' }, { status: 400, statusText: 'Bad Request' });

    const error = await result;

    expect(error.code).toBe('HTTP_400');
    expect(error.message).toBe('Payload inválido.');
  });
});
