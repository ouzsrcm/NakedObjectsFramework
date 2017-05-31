import { ContextService } from '../context.service';
import { Component, Input } from '@angular/core';
import { FieldViewModel } from '../view-models/field-view-model';
import { ChoiceViewModel } from '../view-models/choice-view-model';
import { IDraggableViewModel } from '../view-models/idraggable-view-model';
import { FormGroup, AbstractControl } from '@angular/forms';
import { Dictionary } from 'lodash';
import { BehaviorSubject } from 'rxjs';

@Component({
    selector: 'nof-auto-complete-facade',
    template: require('./auto-complete-facade.component.html'),
    styles: [require('./auto-complete-facade.component.css')]
})
export class AutoCompleteFacadeComponent {

    constructor(private readonly context: ContextService) { }

    private _model: FieldViewModel;

    @Input()
    set model(m: FieldViewModel) {
        this._model = m;
    }

    @Input()
    form: FormGroup;

    get model() {
        return this._model;
    }

    get modelPaneId() {
        return this.model.paneArgId;
    }

    get modelId() {
        return this.model.id;
    }

    get control(): AbstractControl {
        return this.form.controls[this.model.id];
    }

    get choices(): ChoiceViewModel[] {
        return this.model.choices;
    }

    // todo cloned from field component - move to helpers ?

    canDrop = false;

    accept(droppableVm: FieldViewModel) {

        return (draggableVm: IDraggableViewModel) => {
            if (draggableVm) {
                draggableVm.canDropOn(droppableVm.returnType).then((canDrop: boolean) => this.canDrop = canDrop).catch(() => this.canDrop = false);
                return true;
            }
            return false;
        }
    };

    drop(draggableVm: IDraggableViewModel) {
        if (this.canDrop) {
            this.model.drop(draggableVm)
                .then((success) => {
                    this.control.setValue(this.model.selectedChoice);
                });
        }
    }

    classes(): Dictionary<boolean | null> {
        return {
            [this.model.color]: true,
            "candrop": this.canDrop,
            "mat-input-element": null as boolean | null // remove this class to prevent angular/materials styling overiding our styling
        };
    }

    get description() {
        return this.model.description;
    }

    paste(event: KeyboardEvent) {
        const vKeyCode = 86;
        const deleteKeyCode = 46;
        if (event && (event.keyCode === vKeyCode && event.ctrlKey)) {
            const cvm = this.context.getCopyViewModel();

            if (cvm) {
                this.model.drop(cvm)
                    .then((success) => {
                        this.control.setValue(this.model.selectedChoice);
                    });
                event.preventDefault();
            }
        }
        if (event && event.keyCode === deleteKeyCode) {
            this.context.setCopyViewModel(null);
        }
    }

    clear() {
        this.model.clear();
        this.control.reset();
    }

    private bSubject: BehaviorSubject<any>;

    get subject() {
        if (!this.bSubject) {
            const initialValue = this.control.value;
            this.bSubject = new BehaviorSubject(initialValue);

            this.control.valueChanges.subscribe((data) => {
                this.bSubject.next(data);
            });
        }

        return this.bSubject;
    }
}