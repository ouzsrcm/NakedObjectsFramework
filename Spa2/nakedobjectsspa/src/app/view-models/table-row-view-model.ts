﻿import { TableRowColumnViewModel } from './table-row-column-view-model';
import * as _ from "lodash";
import * as Models from '../models';
import { ViewModelFactoryService} from '../view-model-factory.service';

export class TableRowViewModel {

    constructor(private viewModelFactory : ViewModelFactoryService,  properties: _.Dictionary<Models.PropertyMember>, paneId: number) {
        this.properties = _.map(properties, (property, id) => viewModelFactory.propertyTableViewModel(property, id, paneId));
    }

    properties: TableRowColumnViewModel[];

    // todo these currently initialised outside constructor - smell ?
    title: string;
    hasTitle: boolean;
    
    getPlaceHolderTableRowColumnViewModel(id: string) {
        const ph = new TableRowColumnViewModel();
        ph.id = id;
        ph.type = "scalar";
        ph.value = "";
        ph.formattedValue = "";
        ph.title = "";
        return ph;
    }

    conformColumns(columns: string[]) {
        if (columns) {
            this.properties =
                _.map(columns, c => _.find(this.properties, tp => tp.id === c) || this.getPlaceHolderTableRowColumnViewModel(c));
        }
    }
}