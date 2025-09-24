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
import { MatTableModule } from '@angular/material/table';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CapitalGainsService, Operation, TaxResult, CalculationResult, ScenarioInfo } from './services/capital-gains.service';

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
    MatExpansionModule,
    MatTableModule
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
  operations: Operation[] = [];
  scenarios: ScenarioInfo[] = [];
  isCalculating = false;

  displayedColumns: string[] = ['operationNumber', 'operation', 'unitCost', 'quantity', 'total', 'tax', 'status'];
  
  get tableData(): any[] {
    const data: any[] = [];
    
    this.operations.forEach((op, index) => {
      const taxResult = this.taxResults[index] || { tax: 0, hasError: false, error: null };
      data.push({
        operationNumber: index + 1,
        operation: op.operation,
        unitCost: op['unit-cost'],
        quantity: op.quantity,
        total: (op['unit-cost'] * op.quantity),
        tax: taxResult.tax,
        hasError: taxResult.hasError,
        error: taxResult.error,
        status: this.getStatusText(taxResult)
      });
    });

    if (this.operations.length === 0 && this.taxResults.length > 0) {
      this.taxResults.forEach((result, index) => {
        data.push({
          operationNumber: index + 1,
          operation: 'Scenario ' + (index + 1),
          unitCost: '-',
          quantity: '-',
          total: '-',
          tax: result.tax,
          hasError: result.hasError,
          error: result.error,
          status: this.getStatusText(result)
        });
      });
    }

    return data;
  }

  getStatusText(result: TaxResult): string {
    if (result.hasError) return 'Error';
    if (result.tax === 0) return 'Tax Exempt';
    return 'Taxable';
  }

  onTabChange(event: any): void {
    this.operationsJson = '';
    this.selectedFileName = '';
    this.taxResults = [];
    this.operations = [];
    this.scenarios = [];
  }

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

# Only the JSON lines above will be processed`,
    withErrors: `[
  {"operation":"buy", "unit-cost":10.00, "quantity": 100},
  {"operation":"sell", "unit-cost":-15.00, "quantity": 50},
  {"operation":"buy", "unit-cost":20.00, "quantity": -10}
]`
  };

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFileName = file.name;
      
      if (file.name.toLowerCase().endsWith('.txt')) {
        this.uploadFile(file);
      } else {
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
      next: (result: CalculationResult) => {
        this.operations = result.operations;
        this.taxResults = result.taxResults;
        this.scenarios = result.scenarios || [];
        this.isCalculating = false;
        console.log('File upload and processing successful:', result);
      },
      error: (error: any) => {
        console.error('Error uploading file:', error);
        this.isCalculating = false;
        
        if (error.status === 400) {
          alert('File processing error: ' + (error.error || 'Invalid file format or content.'));
        } else {
          alert('Error processing file. Please check the file format and try again.');
        }
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
      
      this.capitalGainsService.calculateCapitalGains(operations).subscribe({
        next: (result) => {
          this.operations = result.operations;
          this.taxResults = result.taxResults;
          this.scenarios = result.scenarios || [];
          this.isCalculating = false;
          console.log('Tax calculation successful:', result);
        },
        error: (error) => {
          console.error('Error calling backend API:', error);
          this.isCalculating = false;
          
          if (error.status === 400) {
            alert('Invalid operations detected: ' + (error.error || 'Please check your input data.'));
          } else {
            console.log('Falling back to local simulation...');
            this.taxResults = this.capitalGainsService.simulateCalculation(operations);
            this.operations = operations;
            alert('API connection failed. Using local calculation as fallback.');
          }
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
    this.operations = [];
    this.scenarios = [];
  }

  loadExample(exampleType: 'basic' | 'complex'): void {
    this.operationsJson = this.examples[exampleType];
    this.selectedFileName = '';
  }

  loadExampleWithErrors(): void {
    this.operationsJson = this.examples.withErrors;
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
