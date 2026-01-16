import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { AnalysisService } from '../../core/services/analysis.service';
import { of, throwError } from 'rxjs';
import { DashboardDto, ExpenseLevel } from '../../core/models/analysis.model';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let mockAnalysisService: jasmine.SpyObj<AnalysisService>;

  const mockDashboardData: DashboardDto = {
    totalIncome: 5000,
    totalExpenses: 2850,
    availableBalance: 2150,
    monthlyExpenses: [
      { month: 'Sep', amount: 1600, level: ExpenseLevel.Medium },
      { month: 'Oct', amount: 800, level: ExpenseLevel.Low },
      { month: 'Nov', amount: 450, level: ExpenseLevel.Low }
    ]
  };

  beforeEach(async () => {
    mockAnalysisService = jasmine.createSpyObj('AnalysisService', ['getDashboard']);
    mockAnalysisService.getDashboard.and.returnValue(of(mockDashboardData));

    await TestBed.configureTestingModule({
      declarations: [ DashboardComponent ],
      providers: [
        { provide: AnalysisService, useValue: mockAnalysisService }
      ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load dashboard data on init', () => {
    fixture.detectChanges();
    expect(mockAnalysisService.getDashboard).toHaveBeenCalledWith(1);
    expect(component.dashboardData).toEqual(mockDashboardData);
  });

  it('should handle error when loading dashboard data', () => {
    mockAnalysisService.getDashboard.and.returnValue(
      throwError({ error: 'Failed to load' })
    );
    fixture.detectChanges();
    expect(component.errorMessage).toBeTruthy();
  });

  it('should return correct CSS class for expense level', () => {
    expect(component.getExpenseLevelClass(ExpenseLevel.Low)).toBe('level-low');
    expect(component.getExpenseLevelClass(ExpenseLevel.Medium)).toBe('level-medium');
    expect(component.getExpenseLevelClass(ExpenseLevel.High)).toBe('level-high');
  });
});
