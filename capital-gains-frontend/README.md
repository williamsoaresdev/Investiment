# 🎨 Capital Gains Frontend - Angular 19

Modern and responsive web interface for calculating taxes on capital gains operations, built with Angular 19 and Material Design.

> **Note**: This is the frontend of the solution. For complete project information, see the [main README](../README.md).

## 🚀 Technology Stack

### **Core Framework**
- **Angular 19**: Modern framework with Standalone Components
- **TypeScript 5+**: Static typing and secure development
- **RxJS**: Reactive programming for state management
- **Angular CLI**: Complete development tooling

### **UI/UX Design**  
- **Angular Material**: UI components following Material Design
- **Responsive Layout**: Adaptive design for mobile/desktop
- **Custom SCSS**: Custom styles and themes
- **Angular CDK**: Advanced utilities for behaviors

### **HTTP & API Integration**
- **HttpClient**: Communication with .NET backend
- **Interceptors**: Centralized error handling  
- **Reactive Forms**: Robust form validation
- **Type-safe**: Typed DTOs for requests/responses

## ⚡ Quick Start

### **Prerequisites**
```bash
# Check versions
node --version    # Node.js 18+
npm --version     # npm 9+  
ng version        # Angular CLI 19+
```

### **Installation and Execution**
```bash
cd capital-gains-frontend

# Install dependencies
npm install

# Development (hot reload)
ng serve

# Application available at:
# http://localhost:4200
```

### **Production Build**
```bash
# Optimized build
ng build --configuration production

# Serve local build
ng serve --configuration production

# Generated files in: dist/capital-gains-frontend/
```

## 🎨 Interface Features

### **📤 File Upload**
- Drag & drop for JSON/TXT files
- Format and content validation  
- Data preview before processing
- Visual feedback for success/error

### **📝 Manual Entry**
- Form for operation insertion
- Real-time field validation
- Auto-complete for operation types
- Automatic value formatting

### **📊 Results Visualization**
- Interactive table with Angular Material
- Sequential operation numbering
- Real-time tax calculation
- Results export (CSV/JSON)

### **🗂️ Tab System**
- Multiple simultaneous scenarios
- Automatic cleanup when switching tabs
- Calculation history per tab
- Scenario comparison

### **📱 Responsive Design**
- Adaptive layout for mobile/tablet/desktop
- Touch-friendly for mobile devices
- Optimized performance for all screens
- Complete accessibility (a11y)

## 🏗️ Project Structure

```
src/
├── app/
│   ├── components/           # Reusable components
│   │   ├── file-upload/      # File upload
│   │   ├── operation-form/   # Operations form
│   │   ├── results-table/    # Results table
│   │   └── tabs/             # Tab system
│   │
│   ├── services/             # Services and HTTP clients
│   │   ├── capital-gains.service.ts  # API communication
│   │   ├── file.service.ts           # File processing
│   │   └── notification.service.ts  # Toast notifications
│   │
│   ├── models/               # Interfaces and DTOs
│   │   ├── operation.model.ts        # Operation model
│   │   ├── tax-result.model.ts       # Tax result
│   │   └── api-response.model.ts     # API responses
│   │
│   ├── shared/               # Shared modules
│   │   ├── material/         # Angular Material imports
│   │   └── validators/       # Custom validators
│   │
│   └── app.component.*       # Root component
│
├── assets/                   # Static resources
├── environments/             # Environment configurations
└── styles.scss              # Global styles
```

## 🧪 Testing and Quality

### **Run Unit Tests**
```bash
# Tests with Karma/Jasmine
ng test

# Tests in watch mode
ng test --watch

# Coverage report
ng test --code-coverage
```

### **End-to-End Tests**
```bash
# E2E with Cypress/Protractor
ng e2e

# Headless mode
ng e2e --headless
```

### **Linting and Formatting**
```bash
# ESLint
ng lint

# Prettier (if configured)
npm run format

# TypeScript check
ng build --aot
```

## 🔧 Development Settings

### **angular.json - Main configurations**
```json
{
  "serve": {
    "builder": "@angular-devkit/build-angular:dev-server",
    "options": {
      "port": 4200,
      "host": "localhost",
      "proxyConfig": "proxy.conf.json"
    }
  }
}
```

### **proxy.conf.json - API Proxy**
```json
{
  "/api/*": {
    "target": "https://localhost:5001",
    "secure": true,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

### **environment.ts - Environment variables**
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  enableDebugTools: true
};
```

## 📦 Main Dependencies

### **Runtime Dependencies**
```json
{
  "@angular/core": "^19.0.0",
  "@angular/material": "^19.0.0",  
  "@angular/cdk": "^19.0.0",
  "@angular/forms": "^19.0.0",
  "rxjs": "^7.8.0"
}
```

### **Development Dependencies**  
```json
{
  "@angular/cli": "^19.0.0",
  "typescript": "~5.6.0",
  "karma": "~6.4.0",
  "jasmine": "~5.1.0"
}
```

## 🎯 Available NPM Scripts

```bash
# Development
npm start              # ng serve
npm run dev            # ng serve with proxy

# Build
npm run build          # ng build
npm run build:prod     # ng build --configuration production

# Tests  
npm run test           # ng test
npm run test:headless  # ng test --browsers ChromeHeadless
npm run e2e           # ng e2e

# Quality
npm run lint          # ng lint
npm run format        # prettier --write
```

## 🌐 Backend Integration

### **Capital Gains Service**
```typescript
@Injectable({
  providedIn: 'root'
})
export class CapitalGainsService {
  constructor(private http: HttpClient) {}

  calculateCapitalGains(operations: Operation[]): Observable<TaxResult[]> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/calculate`, { operations })
      .pipe(
        map(response => response.taxes),
        catchError(this.handleError)
      );
  }

  uploadFile(file: File): Observable<TaxResult[]> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<ApiResponse>(`${this.apiUrl}/upload`, formData);
  }
}
```

### **Global Error Handling**
```typescript
@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // Error logging
        console.error('HTTP Error:', error);
        
        // User notification
        this.notificationService.showError(error.message);
        
        return throwError(() => error);
      })
    );
  }
}
```

## 🚀 Deploy and Production

### **Optimized Build**
```bash
# Production with optimizations
ng build --configuration production

# Production configurations applied:
# • AOT compilation
# • Tree-shaking
# • Minification  
# • Bundle splitting
# • Service Worker (if enabled)
```

### **Docker Frontend**
```dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

COPY . .
RUN ng build --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/capital-gains-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
```

### **Nginx Configuration**
```nginx
server {
  listen 80;
  root /usr/share/nginx/html;
  index index.html;

  location / {
    try_files $uri $uri/ /index.html;
  }

  location /api {
    proxy_pass https://capital-gains-api:5001;
  }
}
```

## 🔮 Next Improvements

### **UX/UI Enhancements**
- 🎨 Dark/Light theme toggle
- 📊 Interactive charts (Chart.js/D3.js)  
- 💾 Local data persistence
- 🔄 Undo/Redo functionality
- 📱 PWA capabilities

### **Performance**
- ⚡ OnPush change detection
- 🧩 Lazy loading modules
- 📦 Bundle optimization
- 🗄️ State management (NgRx)

### **Testing**
- 🧪 Component integration tests
- 🤖 Visual regression testing
- 📊 Performance testing
- ♿ Accessibility testing

---

## 📞 Development

### **Useful Commands**
```bash
# Generate component
ng generate component components/new-component

# Generate service  
ng generate service services/new-service

# Generate guard
ng generate guard guards/auth-guard

# Update Angular
ng update @angular/cli @angular/core
```

### **Debug Tips**
- 🔍 Angular DevTools (Chrome extension)
- 📊 RxJS debugging with `tap()` operator
- 🐛 Source maps enabled in development
- 📱 Device testing with `ng serve --host 0.0.0.0`

*For complete full-stack project information, see the [main README](../README.md).*
