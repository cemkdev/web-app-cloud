import { Injectable } from '@angular/core';
import { HttpClientService } from '../http-client.service';
import { firstValueFrom, Observable } from 'rxjs';
import { EndpointMenu } from '../../../contracts/application-configurations/endpoint-menu';

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {

  constructor(private httpClientService: HttpClientService) { }

  // GETS API endpoint spesifications obtained through reflection during runtime.
  async getAuthorizeDefinitionEndpoints(): Promise<EndpointMenu[]> {
    const observable: Observable<EndpointMenu[]> = this.httpClientService.get<EndpointMenu[]>({
      controller: "ApplicationServices",
      action: "get-authorize-definition-endpoints"
    });
    const menus = await firstValueFrom(observable);
    return menus;
  }
}
