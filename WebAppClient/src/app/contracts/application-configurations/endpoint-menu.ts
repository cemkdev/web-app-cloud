import { ActionType } from '../../enums/application_action_type';

export class EndpointMenu {
    name: string;
    endpoints: EndpointDefinition[];
}

export class EndpointDefinition {
    actionType: ActionType;
    httpType: string;
    definition: string;
    code: string;
    adminOnly: boolean;
}