import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CapitalGainsService, Operation, TaxResult } from './services/capital-gains.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    CommonModule,
    FormsModule,
    MatToolbarModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private capitalGainsService = inject(CapitalGainsService);
  
  title = 'capital-gains-frontend';
  operationsJson = '';
  selectedFileName = '';
  isDragOver = false;
  taxResults: TaxResult[] = [];
  isCalculating = false;

  examples = {
    basic: `[
  {"operation":"buy", "unit-cost":10.00, "quantity": 100},
  {"operation":"sell", "unit-cost":15.00, "quantity": 50},
  {"operation":"sell", "unit-cost":15.00, "quantity": 50}
]`,
    complex: `[
  {"operation":"buy", "unit-cost":20.00, "quantity": 10000},
  {"operation":"sell", "unit-cost":10.00, "quantity": 5000},
  {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
  {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
  {"operation":"sell", "unit-cost":25.00, "quantity": 1000}
]`,
    testFile: `# Capital Gains Test Scenarios
# Comments and metadata are ignored

==================================================
Scenario 1 — Basic operations with exemption
stdin:
[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
stdout:
[{"tax":0.0},{"tax":0.0}]

--------------------------------------------------
Scenario 2 — Taxable profit
stdin:
[{"operation":"buy","unit-cost":10.00,"quantity":10000},{"operation":"sell","unit-cost":20.00,"quantity":5000}]
stdout:
[{"tax":0.0},{"tax":10000.0}]

# Only the JSON lines above will be processed`
  };

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFileName = file.name;
      
      // For test scenario files (.txt), upload directly to backend
      if (file.name.toLowerCase().endsWith('.txt')) {
        this.uploadFile(file);
      } else {
        // For JSON files, read content and display in textarea
        const reader = new FileReader();
        reader.onload = (e) => {
          const content = e.target?.result as string;
          this.operationsJson = content;
        };
        reader.readAsText(file);
      }
    }
  }

  uploadFile(file: File): void {
    this.isCalculating = true;
    
    this.capitalGainsService.uploadFile(file).subscribe({
      next: (results: TaxResult[]) => {
        this.taxResults = results;
        this.isCalculating = false;
        console.log('File upload and processing successful:', results);
      },
      error: (error: any) => {
        console.error('Error uploading file:', error);
        this.isCalculating = false;
        alert('Error processing file. Please check the file format and try again.');
      }
    });
  }

  hasValidInput(): boolean {
    return this.operationsJson.trim().length > 0 && !this.isCalculating;
  }

  calculateTaxes(): void {
    if (!this.hasValidInput()) return;

    this.isCalculating = true;
    
    try {
      const operations: Operation[] = JSON.parse(this.operationsJson);
      
      // Call the real .NET backend API
      this.capitalGainsService.calculateCapitalGains(operations).subscribe({
        next: (results) => {
          this.taxResults = results;
          this.isCalculating = false;
          console.log('Tax calculation successful:', results);
        },
        error: (error) => {
          console.error('Error calling backend API:', error);
          this.isCalculating = false;
          // Fallback to simulation if API fails
          console.log('Falling back to local simulation...');
          this.taxResults = this.capitalGainsService.simulateCalculation(operations);
          alert('API connection failed. Using local calculation as fallback.');
        }
      });
      
    } catch (error) {
      console.error('Error parsing JSON:', error);
      this.isCalculating = false;
      alert('Invalid JSON format. Please check your input.');
    }
  }

  clearInput(): void {
    this.operationsJson = '';
    this.selectedFileName = '';
    this.taxResults = [];
  }

  loadExample(exampleType: 'basic' | 'complex'): void {
    this.operationsJson = this.examples[exampleType];
    this.selectedFileName = '';
  }

  exportResults(): void {
    const dataStr = JSON.stringify(this.taxResults, null, 2);
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr);
    
    const exportFileDefaultName = 'capital-gains-tax-results.json';
    
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', dataUri);
    linkElement.setAttribute('download', exportFileDefaultName);
    linkElement.click();
  }
}
