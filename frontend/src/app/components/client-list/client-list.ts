import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { ClientService } from '../../services/client';
import { Client } from '../../models/client.model';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSortModule
  ],
  templateUrl: './client-list.html',
  styleUrl: './client-list.css',
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class ClientList implements OnInit {
  private clientService = inject(ClientService);

  displayedColumns: string[] = ['id', 'firstName', 'lastName', 'actions'];
  
  dataSource = new MatTableDataSource<Client>([]);
  
  expandedElement: Client | null = null;

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.clientService.getAll().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.dataSource.sort = this.sort;
      },
      error: (err) => console.error('Error loading clients', err)
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  editClient(client: Client, event: Event) {
    event.stopPropagation();
    console.log('Editar', client);
  }

  deleteClient(id: number, event: Event) {
    event.stopPropagation();
    if(confirm('¿Estás seguro de borrar este cliente?')) {
      this.clientService.delete(id).subscribe(() => this.loadClients());
    }
  }
}