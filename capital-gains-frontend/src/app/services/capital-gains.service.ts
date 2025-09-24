import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Operation {
  operation: 'buy' | 'sell';
  'unit-cost': number;
  quantity: number;
}

export interface TaxResult {
  tax?: number;
  error?: string;
  hasError?: boolean;
}

export interface ScenarioInfo {
  scenarioNumber: number;
  operationCount: number;
  resultCount: number;
  operationStartIndex: number;
  resultStartIndex: number;
}

export interface CalculationResult {
  operations: Operation[];
  taxResults: TaxResult[];
  scenarios?: ScenarioInfo[];
}

@Injectable({
  providedIn: 'root'
})
export class CapitalGainsService {
  private readonly baseUrl = 'https://localhost:5001/api/capitalgains'; // Backend .NET API URL (using HTTPS)

  constructor(private http: HttpClient) { }

  calculateCapitalGains(operations: Operation[]): Observable<CalculationResult> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const requestBody = { operations: operations };
    return this.http.post<any>(`${this.baseUrl}/calculate`, requestBody, httpOptions)
      .pipe(
        map((response: any) => ({
          operations: response.operations || operations,
          taxResults: response.taxes || [],
          scenarios: response.scenarios || []
        }))
      );
  }

  uploadFile(file: File): Observable<CalculationResult> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<any>(`${this.baseUrl}/upload`, formData)
      .pipe(
        map((response: any) => ({
          operations: response.operations || [],
          taxResults: response.taxes || [],
          scenarios: response.scenarios || []
        }))
      );
  }

  // For development/testing - simulate the calculation locally
  simulateCalculation(operations: Operation[]): TaxResult[] {
    const results: TaxResult[] = [];
    let weightedAverage = 0;
    let totalShares = 0;
    let accumulatedLoss = 0;

    for (const operation of operations) {
      if (operation.operation === 'buy') {
        const totalCost = (weightedAverage * totalShares) + (operation['unit-cost'] * operation.quantity);
        totalShares += operation.quantity;
        weightedAverage = totalShares > 0 ? totalCost / totalShares : 0;
        results.push({ tax: 0.00 });
      } else if (operation.operation === 'sell') {
        const profit = (operation['unit-cost'] - weightedAverage) * operation.quantity;
        totalShares -= operation.quantity;
        
        let tax = 0;
        if (profit > 0) {
          const taxableProfit = Math.max(0, profit - accumulatedLoss);
          // Apply 20% tax if sale value exceeds R$ 20,000
          if (operation['unit-cost'] * operation.quantity > 20000) {
            tax = taxableProfit * 0.20;
          }
          accumulatedLoss = Math.max(0, accumulatedLoss - profit);
        } else {
          accumulatedLoss += Math.abs(profit);
        }
        
        results.push({ tax: Math.max(0, tax) });
      }
    }
    
    return results;
  }
}