import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '', redirectTo: 'functions', pathMatch: 'full'
  },
  {
    path: 'functions',
    loadChildren: async () => (await import('./functions/functions.module')).FunctionsModule
  },
  {
    path: 'statemachines',
    loadChildren: async () => (await import('./statemachines/statemachines.module')).StateMachinesModule
  },
  {
    path: 'statemachineinstances',
    loadChildren: async () => (await import('./statemachineinstances/statemachineinstances.module')).StateMachineInstancesModule
  },
  {
    path: 'eventmeshservers',
    loadChildren: async () => (await import('./eventmeshservers/eventmeshservers.module')).EventMeshServersModule
  }
];
