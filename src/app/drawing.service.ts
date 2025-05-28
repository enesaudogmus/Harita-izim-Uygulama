import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';


export interface Drawing {
  id?: number;
  name: string;
  geoJson: string;
}

@Injectable({
  providedIn: 'root',
})
export class DrawingService {
  private readonly apiUrl = 'http://localhost:5186/api/drawings';

  constructor(private http: HttpClient) {}


  saveDrawing(drawing: Drawing): Observable<Drawing> {
    return this.http.post<Drawing>(this.apiUrl, drawing).pipe(
      retry(1),
      catchError(this.handleError)
    );
  }

  
  getAllDrawings(): Observable<Drawing[]> {
    return this.http.get<Drawing[]>(this.apiUrl).pipe(
      retry(1),
      catchError(this.handleError)
    );
  }

  
  updateDrawing(id: number, drawing: Drawing): Observable<Drawing> {
    return this.http.put<Drawing>(`${this.apiUrl}/${id}`, drawing).pipe(
      retry(1),
      catchError(this.handleError)
    );
  }

  
  deleteDrawing(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      retry(1),
      catchError(this.handleError)
    );
  }

  
  private handleError(error: HttpErrorResponse) {
    let errorMsg = '';
    if (error.error instanceof ErrorEvent) {
     
      errorMsg = `İstemci hatası: ${error.error.message}`;
    } else {
      
      errorMsg = `Sunucu hatası: ${error.status} - ${error.message}`;
    }

    
    console.error(errorMsg);
    return throwError(() => new Error(errorMsg));
  }
}
