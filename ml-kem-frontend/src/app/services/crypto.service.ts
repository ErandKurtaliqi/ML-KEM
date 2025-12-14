import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type MlKemLevel = 'MLKem512' | 'MLKem768' | 'MLKem1024';

export interface KeyPairResponse {
  publicKeyBase64: string;
  privateKeyBase64: string;
  algorithm: string;
  publicKeySize: number;
  privateKeySize: number;
}

export interface EncryptRequest {
  text: string;
  receiverPublicKeyBase64: string;
  level: MlKemLevel;
}

export interface EncryptResponse {
  mlKemAlgorithm: string;
  receiverPublicKeyBase64: string;
  kemCiphertextBase64: string;
  nonceBase64: string;
  ciphertextBase64: string;
  tagBase64: string;
  plaintextLength: number;
}

export interface DecryptRequest {
  receiverPrivateKeyBase64: string;
  kemCiphertextBase64: string;
  nonceBase64: string;
  ciphertextBase64: string;
  tagBase64: string;
  level: MlKemLevel;
}

export interface DecryptResponse {
  decryptedText: string;
  algorithm: string;
}

export interface AlgorithmInfo {
  name: string;
  level: MlKemLevel;
  securityLevel: string;
  description: string;
  publicKeySize: number;
  privateKeySize: number;
  ciphertextSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  generateKeys(level: MlKemLevel): Observable<KeyPairResponse> {
    return this.http.get<KeyPairResponse>(`${this.baseUrl}/keys/${level}`);
  }

  encrypt(request: EncryptRequest): Observable<EncryptResponse> {
    return this.http.post<EncryptResponse>(`${this.baseUrl}/encrypt`, request);
  }

  decrypt(request: DecryptRequest): Observable<DecryptResponse> {
    return this.http.post<DecryptResponse>(`${this.baseUrl}/decrypt`, request);
  }

  getAlgorithms(): Observable<AlgorithmInfo[]> {
    return this.http.get<AlgorithmInfo[]>(`${this.baseUrl}/algorithms`);
  }
}

