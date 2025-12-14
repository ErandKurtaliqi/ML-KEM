import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { 
  CryptoService, 
  MlKemLevel, 
  KeyPairResponse, 
  EncryptResponse,
  AlgorithmInfo 
} from './services/crypto.service';

interface KeyPair {
  publicKey: string;
  privateKey: string;
  algorithm: string;
  level: MlKemLevel;
  publicKeySize: number;
  privateKeySize: number;
}

interface EncryptedMessage {
  kemCiphertextBase64: string;
  nonceBase64: string;
  ciphertextBase64: string;
  tagBase64: string;
  algorithm: string;
  plaintextLength: number;
  timestamp: Date;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  // Tab state
  activeTab: 'encrypt' | 'decrypt' | 'keys' | 'info' = 'keys';
  
  // Security levels
  levels: { value: MlKemLevel; label: string; bits: string }[] = [
    { value: 'MLKem512', label: 'ML-KEM-512', bits: '128-bit' },
    { value: 'MLKem768', label: 'ML-KEM-768', bits: '192-bit' },
    { value: 'MLKem1024', label: 'ML-KEM-1024', bits: '256-bit' }
  ];
  
  selectedLevel: MlKemLevel = 'MLKem768';
  
  // Key generation
  generatedKeyPair: KeyPair | null = null;
  isGeneratingKeys = false;
  
  // Encryption
  plaintext = '';
  recipientPublicKey = '';
  encryptionLevel: MlKemLevel = 'MLKem768';
  encryptedResult: EncryptedMessage | null = null;
  isEncrypting = false;
  
  // Decryption
  privateKeyForDecryption = '';
  kemCiphertext = '';
  nonce = '';
  ciphertext = '';
  tag = '';
  decryptionLevel: MlKemLevel = 'MLKem768';
  decryptedText = '';
  isDecrypting = false;
  
  // Algorithm info
  algorithms: AlgorithmInfo[] = [];
  
  // UI State
  copyFeedback: { [key: string]: boolean } = {};
  errorMessage = '';
  successMessage = '';

  constructor(private cryptoService: CryptoService) {}

  ngOnInit(): void {
    this.loadAlgorithms();
  }

  loadAlgorithms(): void {
    this.cryptoService.getAlgorithms().subscribe({
      next: (algos) => {
        this.algorithms = algos;
      },
      error: (err) => {
        console.error('Error loading algorithms:', err);
      }
    });
  }

  setTab(tab: 'encrypt' | 'decrypt' | 'keys' | 'info'): void {
    this.activeTab = tab;
    this.clearMessages();
  }

  generateKeyPair(): void {
    this.isGeneratingKeys = true;
    this.clearMessages();
    
    this.cryptoService.generateKeys(this.selectedLevel).subscribe({
      next: (response: KeyPairResponse) => {
        this.generatedKeyPair = {
          publicKey: response.publicKeyBase64,
          privateKey: response.privateKeyBase64,
          algorithm: response.algorithm,
          level: this.selectedLevel,
          publicKeySize: response.publicKeySize,
          privateKeySize: response.privateKeySize
        };
        this.isGeneratingKeys = false;
        this.showSuccess('Çelësat u gjeneruan me sukses!');
      },
      error: (err) => {
        this.isGeneratingKeys = false;
        this.showError('Gabim gjatë gjenerimit të çelësave: ' + (err.error?.error || err.message));
      }
    });
  }

  encryptMessage(): void {
    if (!this.plaintext || !this.recipientPublicKey) {
      this.showError('Ju lutem plotësoni të gjitha fushat!');
      return;
    }
    
    this.isEncrypting = true;
    this.clearMessages();
    
    this.cryptoService.encrypt({
      text: this.plaintext,
      receiverPublicKeyBase64: this.recipientPublicKey.trim(),
      level: this.encryptionLevel
    }).subscribe({
      next: (response: EncryptResponse) => {
        this.encryptedResult = {
          kemCiphertextBase64: response.kemCiphertextBase64,
          nonceBase64: response.nonceBase64,
          ciphertextBase64: response.ciphertextBase64,
          tagBase64: response.tagBase64,
          algorithm: response.mlKemAlgorithm,
          plaintextLength: response.plaintextLength,
          timestamp: new Date()
        };
        this.isEncrypting = false;
        this.showSuccess('Mesazhi u enkriptua me sukses!');
      },
      error: (err) => {
        this.isEncrypting = false;
        this.showError('Gabim gjatë enkriptimit: ' + (err.error?.error || err.message));
      }
    });
  }

  decryptMessage(): void {
    if (!this.privateKeyForDecryption || !this.kemCiphertext || !this.nonce || !this.ciphertext || !this.tag) {
      this.showError('Ju lutem plotësoni të gjitha fushat!');
      return;
    }
    
    this.isDecrypting = true;
    this.clearMessages();
    
    this.cryptoService.decrypt({
      receiverPrivateKeyBase64: this.privateKeyForDecryption.trim(),
      kemCiphertextBase64: this.kemCiphertext.trim(),
      nonceBase64: this.nonce.trim(),
      ciphertextBase64: this.ciphertext.trim(),
      tagBase64: this.tag.trim(),
      level: this.decryptionLevel
    }).subscribe({
      next: (response) => {
        this.decryptedText = response.decryptedText;
        this.isDecrypting = false;
        this.showSuccess('Mesazhi u dekriptua me sukses!');
      },
      error: (err) => {
        this.isDecrypting = false;
        this.showError('Gabim gjatë dekriptimit: ' + (err.error?.error || err.message));
      }
    });
  }

  useGeneratedKeys(): void {
    if (this.generatedKeyPair) {
      this.recipientPublicKey = this.generatedKeyPair.publicKey;
      this.encryptionLevel = this.generatedKeyPair.level;
      this.setTab('encrypt');
      this.showSuccess('Çelësi publik u vendos për enkriptim!');
    }
  }

  fillDecryptionFromResult(): void {
    if (this.encryptedResult && this.generatedKeyPair) {
      this.kemCiphertext = this.encryptedResult.kemCiphertextBase64;
      this.nonce = this.encryptedResult.nonceBase64;
      this.ciphertext = this.encryptedResult.ciphertextBase64;
      this.tag = this.encryptedResult.tagBase64;
      this.privateKeyForDecryption = this.generatedKeyPair.privateKey;
      this.decryptionLevel = this.generatedKeyPair.level;
      this.setTab('decrypt');
      this.showSuccess('Të dhënat u plotësuan për dekriptim!');
    }
  }

  async copyToClipboard(text: string, key: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(text);
      this.copyFeedback[key] = true;
      setTimeout(() => {
        this.copyFeedback[key] = false;
      }, 2000);
    } catch (err) {
      this.showError('Gabim gjatë kopjimit!');
    }
  }

  clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
    setTimeout(() => this.errorMessage = '', 5000);
  }

  showSuccess(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
    setTimeout(() => this.successMessage = '', 3000);
  }

  truncateKey(key: string, length: number = 50): string {
    if (key.length <= length) return key;
    return key.substring(0, length) + '...';
  }

  formatBytes(bytes: number): string {
    return `${bytes} bytes`;
  }

  getLevelColor(level: MlKemLevel): string {
    switch (level) {
      case 'MLKem512': return 'var(--warning)';
      case 'MLKem768': return 'var(--primary)';
      case 'MLKem1024': return 'var(--success)';
      default: return 'var(--primary)';
    }
  }

  clearEncryption(): void {
    this.plaintext = '';
    this.encryptedResult = null;
  }

  clearDecryption(): void {
    this.privateKeyForDecryption = '';
    this.kemCiphertext = '';
    this.nonce = '';
    this.ciphertext = '';
    this.tag = '';
    this.decryptedText = '';
  }
}
