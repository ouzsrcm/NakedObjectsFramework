﻿/// <reference path="nakedobjects.gemini.services.urlmanager.ts" />

module NakedObjects.Angular.Gemini {

    import IResourceRepresentation = NakedObjects.RoInterfaces.IResourceRepresentation;

    export abstract class Command {

        constructor(protected urlManager: IUrlManager,
            protected nglocation: ng.ILocationService,
            protected commandFactory: ICommandFactory,
            protected context: IContext,
            protected navigation: INavigation,
            protected $q: ng.IQService,
            protected $route: ng.route.IRouteService
        ) { }

        public fullCommand: string;
        public helpText: string;
        protected minArguments: number;
        protected maxArguments: number;
        protected vm: CiceroViewModel;

        //Must be called after construction and before execute is called
        initialiseWithViewModel(cvm: CiceroViewModel) {
            this.vm = cvm;
        }

        public execute(argString: string, chained: boolean): void {
            if (!this.isAvailableInCurrentContext()) {
                this.clearInputAndSetMessage("The command: " + this.fullCommand + " is not available in the current context");
                return;
            }
            //TODO: This could be moved into a pre-parse method as it does not depend on context
            if (argString == null) {
                if (this.minArguments > 0) {
                    this.clearInputAndSetMessage("No arguments provided");
                    return;
                }
            } else {
                const args = argString.split(",");
                if (args.length < this.minArguments) {
                    this.clearInputAndSetMessage("Too few arguments provided");
                    return;
                }
                else if (args.length > this.maxArguments) {
                    this.clearInputAndSetMessage("Too many arguments provided");
                    return;
                }
            }
            this.doExecute(argString, chained);
        }

        abstract doExecute(args: string, chained: boolean): void;

        public abstract isAvailableInCurrentContext(): boolean;

       
        //Helper methods follow
        protected clearInputAndSetMessage(text: string): void {
            this.vm.clearInput();
            this.vm.message = text;
            this.$route.reload();
        }

        protected mayNotBeChained(rider: string = ""): void {
            this.clearInputAndSetMessage(this.fullCommand + " command may not be chained" + rider + ". Use Where command to see where execution stopped.");
        }

        protected appendAsNewLineToOutput(text: string): void {
            this.vm.output.concat("/n" + text);
        }

        public checkMatch(matchText: string): void {
            if (this.fullCommand.indexOf(matchText) != 0) {
                throw new Error("No such command: " + matchText);
            }
        }

        //argNo starts from 0.
        //If argument does not parse correctly, message will be passed to UI
        //and command aborted.
        protected argumentAsString(argString: string, argNo: number, optional: boolean = false, toLower: boolean = true): string {
            if (!argString) return undefined;
            if (!optional && argString.split(",").length < argNo + 1) {
                throw new Error("Too few arguments provided");
            }
            var args = argString.split(",");
            if (args.length < argNo + 1) {
                if (optional) {
                    return undefined;
                } else {
                    throw new Error("Required argument number " + (argNo + 1).toString + " is missing");
                }
            }
            return toLower ? args[argNo].trim().toLowerCase() : args[argNo].trim();  // which may be "" if argString ends in a ','
        }

        //argNo starts from 0.
        protected argumentAsNumber(args: string, argNo: number, optional: boolean = false): number {
            const arg = this.argumentAsString(args, argNo, optional);
            if (!arg && optional === true) return null;
            const number = parseInt(arg);
            if (isNaN(number)) {
                throw new Error("Argument number " + (argNo + 1).toString() + " must be a number");
            }
            return number;
        }

        protected parseInt(input: string): number {
            if (!input || input === "") {
                return null;
            }
            const number = parseInt(input);
            if (isNaN(number)) {
                throw new Error(input + " is not a number");
            }
            return number;
        }

        //Parses '17, 3-5, -9, 6-' into two numbers 
        protected parseRange(arg: string): { start: number, end: number } {
            if (!arg) {
                arg = "1-";
            }
            const clauses = arg.split("-");
            const range = { start: null, end: null };
            switch (clauses.length) {
                case 1:
                    range.start = this.parseInt(clauses[0]);
                    range.end = range.start;
                    break;
                case 2:
                    range.start = this.parseInt(clauses[0]);
                    range.end = this.parseInt(clauses[1]);
                    break;
                default:
                    throw new Error("Cannot have more than one dash in argument");
            }
            if ((range.start != null && range.start < 1) || (range.end != null && range.end < 1)) {
                throw new Error("Item number or range values must be greater than zero");
            }
            return range;
        }

        protected getContextDescription(): string {
            //todo
            return null;
        }

        protected routeData(): PaneRouteData {
            return this.urlManager.getRouteData().pane1;
        }
        //Helpers delegating to RouteData
        protected isHome(): boolean {
            return this.vm.viewType === ViewType.Home;
        }
        protected isObject(): boolean {
            return this.vm.viewType === ViewType.Object;
        }
        protected getObject(): ng.IPromise<DomainObjectRepresentation> {
            const oid = this.routeData().objectId;
            return this.context.getObjectByOid(1, oid);
        }
        protected isList(): boolean {
            return this.vm.viewType === ViewType.List;
        }
        protected getList(): ng.IPromise<ListRepresentation> {
            const routeData = this.routeData();
            //TODO: Currently covers only the list-from-menu; need to cover list from object action
            return this.context.getListFromMenu(1, routeData.menuId, routeData.actionId, routeData.actionParams, routeData.page, routeData.pageSize);
        }
        protected isMenu(): boolean {
            return !!this.routeData().menuId;
        }
        protected getMenu(): ng.IPromise<MenuRepresentation> {
            return this.context.getMenu(this.routeData().menuId);
        }
        protected isDialog(): boolean {
            return !!this.routeData().dialogId;
        }

        protected getActionForCurrentDialog(): ng.IPromise<ActionMember> {
            const dialogId = this.routeData().dialogId;
            if (this.isObject()) {
                return this.getObject().then((obj: DomainObjectRepresentation) => {
                    return this.$q.when(obj.actionMember(dialogId));
                });
            } else if (this.isMenu()) {
                return this.getMenu().then((menu: MenuRepresentation) => {
                    return this.$q.when(menu.actionMember(dialogId)); //i.e. return a promise
                });
            }
            return this.$q.reject("List actions not implemented yet");
        }
        //Tests that at least one collection is open (should only be one). 
        //TODO: assumes that closing collection removes it from routeData NOT sets it to Summary
        protected isCollection(): boolean {
            return this.isObject() && _.any(this.routeData().collections);
        }
        protected closeAnyOpenCollections() {
            const open = openCollectionIds(this.routeData());
            _.forEach(open, id => {
                this.urlManager.setCollectionMemberState(1, id, CollectionViewState.Summary);
            });
        }
        protected isTable(): boolean {
            return false; //TODO
        }
        protected isEdit(): boolean {
            return this.routeData().edit;
        }
        protected isTransient(): boolean {
            return this.routeData().transient;
        }

        protected matchingProperties(
            obj: DomainObjectRepresentation,
            match: string): PropertyMember[] {
            let props = _.map(obj.propertyMembers(), prop => prop);
            if (match) {
                props = this.matchFriendlyNameAndOrMenuPath(props, match);
            }
            return props;
        }

        protected matchingCollections(
            obj: DomainObjectRepresentation,
            match: string): CollectionMember[] {
            const allColls = _.map(obj.collectionMembers(), action => action);
            if (match) {
                return this.matchFriendlyNameAndOrMenuPath<CollectionMember>(allColls, match);
            } else {
                return allColls;
            }
        }

        protected matchingParameters(
            action: ActionMember,
            match: string): Parameter[] {
            let params = _.map(action.parameters(), p => p);
            if (match) {
                params = this.matchFriendlyNameAndOrMenuPath(params, match);
            }
            return params;
        }

        protected matchFriendlyNameAndOrMenuPath<T extends IHasExtensions>(
            reps: T[], match: string): T[] {
            const clauses = match.split(" ");
            //An exact match has preference over any partial match
            const exactMatches = _.filter(reps, (rep) => {
                const path = rep.extensions().menuPath();
                const name = rep.extensions().friendlyName().toLowerCase();
                return match === name ||
                    (!!path && match === path.toLowerCase() + " " + name) ||
                    _.all(clauses, clause => {
                        name === clause ||
                        (!!path && path.toLowerCase() === clause)
                    });
            });
            if (exactMatches.length > 0) return exactMatches;
            return _.filter(reps, (rep) => {
                const path = rep.extensions().menuPath();
                const name = rep.extensions().friendlyName().toLowerCase();
                return _.all(clauses, clause => name.indexOf(clause) >= 0 ||
                    (!!path && path.toLowerCase().indexOf(clause) >= 0));
            });
        }

        protected findMatchingChoices(choices: _.Dictionary<Value>, titleMatch: string): Value[] {
            return _.filter(choices, v => v.toString().toLowerCase().indexOf(titleMatch.toLowerCase()) >= 0);
        }

        protected handleErrorResponse(err: ErrorMap, getFriendlyName: (id: string) => string) {
            if (err.invalidReason()) {
                this.clearInputAndSetMessage(err.invalidReason());
                return;
            }
            let msg = "Please complete or correct these fields:\n";
            _.each(err.valuesMap(), (errorValue, fieldId) => {
                msg += this.fieldValidationMessage(errorValue, () => getFriendlyName(fieldId));
            });
            this.clearInputAndSetMessage(msg);
        }

        private fieldValidationMessage(errorValue: ErrorValue, fieldFriendlyName: () => string): string {
            let msg = "";
            const reason = errorValue.invalidReason;
            const value = errorValue.value;
            if (reason) {
                msg += fieldFriendlyName() + ": ";
                if (reason === "Mandatory") {
                    msg += "required";
                } else {
                    msg += value + " " + reason;
                }
                msg += "\n";
            }
            return msg;
        }

        protected valueForUrl(val: Value, field: IField): Value {

            const fieldEntryType = field.entryType();

            if (fieldEntryType !== EntryType.FreeForm  || field.isCollectionContributed()) {

                if (fieldEntryType === EntryType.MultipleChoices || field.isCollectionContributed()) {

                    //Original code from ValueViewModel#getValue(): 
                    //const selections = this.multiChoices || [];
                    //if (val.isScalar()) {
                    //    //TODO: Have to get values from the routeData
                    //    const selValues = _.map(selections, cvm => cvm.value);
                    //    return new Value(selValues);
                    //}

                    //TODO: Reference only -  not multi-select scalars - see above

                    const newVal = new  Value({ href: val.link().href(), title: val.link().title() });
                    let values: Value[];
                    if (field instanceof Parameter) {
                        values = this.routeData().dialogFields[field.parameterId()].list();
                    } else if (field instanceof PropertyMember) {
                        values = this.routeData().props[field.propertyId()].list();
                    }
                    const index = _.findIndex(values, v => v.link().href() === newVal.link().href());
                    if (index > -1) {
                        values.splice(index, 1);
                    } else {
                        values.push(newVal);
                    }
                    const hrefsAndTitles = _.map(values, v => ({ href: v.link().href(), title: v.link().title() }));
                    return new Value(hrefsAndTitles);
                }

                //Single values ...
                if (val.isScalar()) {
                    return val;
                }
                // reference 
                return new Value({ href: val.link().href(), title: val.link().title() });
            }

            if (val.isScalar()) {
                if (val.isNull()) {
                    return new Value("");
                }
                return val;
                //TODO: consider these options:
                //    if (from.value instanceof Date) {
                //        return new Value((from.value as Date).toISOString());
                //    }

                //    return new Value(from.value as number | string | boolean);
            }

            // reference
            return new Value(val.isReference() ? { href: val.link().href(), title: val.link().title() } : null);
        }
    }

    export class Action extends Command {

        public fullCommand = "action";
        public helpText = "Open the dialog for action from a menu, or from object actions. " +
        "Note that a dialog is always opened for an action, even if it has no fields (parameters) - " +
        "this is a safety mechanism, allowing the user to confirm that the action is the one intended." +
        "Once any fields have been completed, using the Field command, the action may then be invoked " +
        "with the OK command." +
        "The action command takes two optional arguments. " +
        "The first is the name, or partial name, of the action. " +
        "If the partial name matches more than one action, a list of matches is returned," +
        "but none opened. If no argument is provided, a full list of available action names is returned. " +
        "The partial name may have more than one clause, separated by spaces, and these may match either " +
        "part(s) of the action name or the sub-menu name if one exists. " +
        "Not yet implemented: if the action name matches a single action, then a question-mark may be added as a second "
        "parameter - which will generate a more detailed description of the Action.";

        protected minArguments = 0;
        protected maxArguments = 2;

        public isAvailableInCurrentContext(): boolean {
            return (this.isMenu() || this.isObject()) && !this.isDialog() && !this.isEdit(); //TODO add list
        }

        doExecute(args: string, chained: boolean): void {
            const match = this.argumentAsString(args, 0);
            const p1 = this.argumentAsString(args, 1, true);
            if (p1) {
                this.clearInputAndSetMessage("Second argument for action is not yet supported");
                return;
            }
            if (this.isObject()) {
                this.getObject()
                    .then((obj: DomainObjectRepresentation) => {
                        this.processActions(match, obj.actionMembers());
                    });
            }
            else if (this.isMenu()) {
                this.getMenu()
                    .then((menu: MenuRepresentation) => {
                        this.processActions(match, menu.actionMembers());
                    });
            }
            //TODO: handle list
        }

        private processActions(match: string, actionsMap: _.Dictionary<ActionMember>) {
            var actions = _.map(actionsMap, action => action);
            if (actions.length === 0) {
                this.clearInputAndSetMessage("No actions available");
                return;
            }
            if (match) {
                actions = this.matchFriendlyNameAndOrMenuPath(actions, match);
            }
            switch (actions.length) {
                case 0:
                    this.clearInputAndSetMessage(match + " does not match any actions");
                    break;
                case 1:
                    const action = actions[0];
                    if (action.disabledReason()) {
                        this.disabledAction(action);
                    } else {
                        this.openActionDialog(action);
                    }
                    break;
                default:
                    let output = match ? "Matching actions:\n" : "Actions:\n";
                    output += this.listActions(actions);
                    this.clearInputAndSetMessage(output);
            }
        }

        private disabledAction(action: ActionMember) {
            let output = "Action: ";
            output += action.extensions().friendlyName() + " is disabled. ";
            output += action.disabledReason();
            this.clearInputAndSetMessage(output);
        }

        private listActions(actions: ActionMember[] ): string {
            return _.reduce(actions, (s, t) => {
                const menupath = t.extensions().menuPath() ? t.extensions().menuPath() + " - " : "";
                const disabled = t.disabledReason() ? " (disabled: " + t.disabledReason() + ")" : "";
                return s + menupath + t.extensions().friendlyName() + disabled + "\n";
            }, "");
        }

        private openActionDialog(action: ActionMember) {
            this.urlManager.setDialog(action.actionId(), 1);  //1 = pane 1
            _.forEach(action.parameters(), (p) => {
                const pVal = this.valueForUrl(p.default(), p);
                this.urlManager.setFieldValue(action.actionId(), p, pVal, 1, false);
            });
        }
    }
    export class Back extends Command {

        public fullCommand = "back";
        public helpText = "Move back to the previous context.";
        protected minArguments = 0;
        protected maxArguments = 0;

        public isAvailableInCurrentContext(): boolean {
            return true;
        }

        doExecute(args: string, chained: boolean): void {
            this.navigation.back();
        };
    }
    export class Cancel extends Command {

        public fullCommand = "cancel";
        public helpText = "Leave the current activity (action dialog, or object edit), incomplete.";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isDialog() || this.isEdit();
        }

        doExecute(args: string, chained: boolean): void {
            if (this.isEdit()) {
                this.urlManager.setObjectEdit(false, 1);
            }
            if (this.isDialog()) {
                this.urlManager.closeDialog(1);
            }
        };
    }
    export class Clipboard extends Command {

        public fullCommand = "clipboard";
        public helpText = "The clipboard command is used for temporarily " +
        "holding a reference to an object, so that it may be used later to enter into a field. " +
        "Clipboard requires one argument, which may take one of four values: " +
        "copy, show, go, or discard, each of which may be abbreviated down to one character. " +
        "Copy copies a reference to the object being viewed into the clipboard, overwriting any existing reference." +
        "Show displays the content of the clipboard without using it or changing context." +
        "Go takes you directly to the object held in the clipboard."
        "Discard removes any existing reference from the clipboard."
        "The reference held in the clipboard may be used within the Field command.";

        protected minArguments = 1;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return true;
        }

        doExecute(args: string, chained: boolean): void {
            const sub = this.argumentAsString(args, 0);
            if ("copy".indexOf(sub) === 0) {
                this.copy();
            } else if ("show".indexOf(sub) === 0) {
                this.show();
            } else if ("go".indexOf(sub) === 0) {
                this.go();
            } else if ("discard".indexOf(sub) === 0) {
                this.discard();
            } else {
                this.clearInputAndSetMessage("Clipboard command may only be followed by copy, show, go, or discard");
            }
        };

        private copy(): void {
            if (!this.isObject()) {
                this.clearInputAndSetMessage("Clipboard copy may only be used in the context of viewing an object");
                return;
            }
            this.getObject().then((obj: DomainObjectRepresentation) => {
                this.vm.clipboard = obj;
                this.show();
            });
        }
        private show(): void {
            if (this.vm.clipboard) {
                const label = Helpers.typePlusTitle(this.vm.clipboard);
                this.clearInputAndSetMessage("Clipboard contains: " + label);
            } else {
                this.clearInputAndSetMessage("Clipboard is empty");
            }
        }
        private go(): void {
            const link = this.vm.clipboard.selfLink();
            if (link) {
                this.urlManager.setItem(link, 1);
            } else {
                this.show();
            }
        }

        private discard(): void {
            this.vm.clipboard = null;
            this.show();
        }
    }
    export class Edit extends Command {

        public fullCommand = "edit";
        public helpText = "Put an object into Edit mode.";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isObject() && !this.isEdit();
        }

        doExecute(args: string, chained: boolean): void {
            if (chained) {
                this.mayNotBeChained();
                return;
            }
            this.urlManager.setObjectEdit(true, 1);
        };
    }
    export class Enter extends Command {

        public fullCommand = "enter";
        public helpText = "Enter a value into a field -  " +
        " meaning a parameter in an action dialog, " +
        "or  a property on an object being edited." +
        "Enter requires 2 arguments. " +
        "The first argument is the partial field name, which must match a single field. " +
        "The second optional argument specifies the value, or selection, to be entered. " +
        "If a question mark is provided as the second argument, the field will not be " +
        "updated but further details will be provided about that input field." +
        "If the word paste is used as the second argument, then, provided that the field is " +
        "a reference field, the object reference in the clipboard will be pasted into the field.";
        protected minArguments = 2;
        protected maxArguments = 2;

        isAvailableInCurrentContext(): boolean {
            return this.isDialog() || this.isEdit() || this.isTransient();
        }

        doExecute(args: string, chained: boolean): void {
            const fieldName = this.argumentAsString(args, 0);
            const fieldEntry = this.argumentAsString(args, 1, false, false);
            if (this.isDialog()) {
                this.fieldEntryForDialog(fieldName, fieldEntry);
            }
            else {
                this.fieldEntryForEdit(fieldName, fieldEntry)
            }
        };

        private fieldEntryForEdit(fieldName: string, fieldEntry: string) {
            this.getObject()
                .then((obj: DomainObjectRepresentation) => {
                    var fields = this.matchingProperties(obj, fieldName);
                    var s: string = "";
                    switch (fields.length) {
                        case 0:
                            s = fieldName + " does not match any fields";
                            break;
                        case 1:
                            const field = fields[0];
                            if (fieldEntry === "?") {
                                s = this.renderPropertyDetails(field);
                            } else {
                                this.setField(field, fieldEntry);
                                return;
                            }
                            break;
                        default:
                            s = fieldName + " matches multiple fields:\n";
                            s += _.reduce(fields, (s, prop) => {
                                return s + prop.extensions().friendlyName() + "\n";
                            }, "");
                    }
                    this.clearInputAndSetMessage(s);
                });
        }

        private fieldEntryForDialog(fieldName: string, fieldEntry: string) {
            this.getActionForCurrentDialog().then((action: ActionMember) => {
                let params = _.map(action.parameters(), param => param);
                params = this.matchFriendlyNameAndOrMenuPath(params, fieldName);
                switch (params.length) {
                    case 0: //TODO: Reword messages to refer specifically to dialog fields? (search for wrong use of 'parameter' elsewhere)
                        this.clearInputAndSetMessage("No fields in the current context match " + fieldName);
                        break;
                    case 1:
                        this.setField(params[0], fieldEntry);
                        break;
                    default:
                        this.clearInputAndSetMessage("Multiple fields match " + fieldName); //TODO: list them
                        break;
                }
            });
        }

        private setField(field: IField, fieldEntry: string): void {
            //TODO: Handle '?' e.g.  this.renderDetails(fieldName);
            const fieldEntryType = field.entryType();
            if ( fieldEntryType === EntryType.Choices || fieldEntryType === EntryType.MultipleChoices ) {
                this.handleChoices(field, fieldEntry);
                return;
            }
            if (field.isScalar()) {
                this.setFieldValue(field, new Value(fieldEntry));
            } else {
                this.handleReferenceField(field, fieldEntry);
            }
        }

        private setFieldValue(field: IField, value: Value) {
            const urlVal = this.valueForUrl(value, field);
            if (field instanceof Parameter) {
                this.urlManager.setFieldValue(this.routeData().dialogId, field, urlVal, 1);
            } else if (field instanceof PropertyMember) {
                const parent = field.parent
                if (parent instanceof DomainObjectRepresentation) {
                    this.urlManager.setPropertyValue(parent, field, urlVal, 1);
                }
            }
        }

        private handleReferenceField(field: IField, fieldEntry: string) {
            if ("paste".indexOf(fieldEntry) === 0) {
                this.handleClipboard(field);
            } else {
                this.clearInputAndSetMessage("Invalid entry for a reference field. Use clipboard or clip");
            }
        }

        private handleClipboard(field: IField) {
            const ref = this.vm.clipboard;
            if (!ref) {
                this.clearInputAndSetMessage("Cannot use Clipboard as it is empty");
                return;
            }
            const paramType = field.extensions().returnType();
            const refType = ref.domainType();
            this.context.isSubTypeOf(refType, paramType)
                .then((isSubType: boolean) => {
                    if (isSubType) {
                        const obj = this.vm.clipboard;
                        const selfLink = obj.selfLink();
                        //Need to add a title to the SelfLink as not there by default
                        selfLink.setTitle(obj.title());
                        const value = new Value(selfLink);
                        this.setFieldValue(field, value);
                    } else {
                        this.clearInputAndSetMessage("Contents of Clipboard are not compatible with the field");
                    }
                });
        }

        private handleChoices(field: IField, fieldEntry: string): void {
            const matches = this.findMatchingChoices(field.choices(), fieldEntry);
            switch (matches.length) {
                case 0:
                    this.clearInputAndSetMessage("None of the choices matches " + fieldEntry);
                    break;
                case 1:
                    this.setFieldValue(field, matches[0]);
                    break;
                default:
                    let msg = "Multiple matches:\n";
                    _.forEach(matches, m => msg += m.toString() + "\n");
                    this.clearInputAndSetMessage(msg);
                    break;
            }
        }

        private renderParameterDetails(fieldName: string, action: ActionMember) {
            let output = "";
            _.forEach(this.routeData().dialogFields, (value, key) => {
                output += Helpers.friendlyNameForParam(action, key) + ": ";
                output += value.toValueString() || "empty";
                output += "\n";
            });
            this.clearInputAndSetMessage(output);
            return;
        }

        private renderPropertyDetails(field: PropertyMember) {
            let s = "Field name: " + field.extensions().friendlyName();
            s += "\nValue: ";
            s += field.value().toString() || "empty";
            s += "\nType: " + Helpers.friendlyTypeName(field.extensions().returnType());
            if (field.disabledReason()) {
                s += "\nUnmodifiable: " + field.disabledReason();
            } else {
                s += field.extensions().optional() ? "\nOptional" : "\nMandatory";
                if (field.choices()) {
                    var label = "\nChoices: ";
                    s += _.reduce(field.choices(), (s, cho) => {
                        return s + cho + " ";
                    }, label);
                }
                const desc = field.extensions().description()
                s += desc ? "\nDescription: " + desc : "";
                //TODO:  Add a Can Paste if clipboard has compatible type
            }
            return s;
        }
    }
    export class Forward extends Command {

        public fullCommand = "forward";
        public helpText = "Move forward to next context in the history (if you have previously moved back).";
        protected minArguments = 0;
        protected maxArguments = 0;

        public isAvailableInCurrentContext(): boolean {
            return true;
        }
        doExecute(args: string, chained: boolean): void {
            this.vm.clearInput();  //To catch case where can't go any further forward and hence url does not change.
            this.navigation.forward();
        };
    }
    export class Gemini extends Command {

        public fullCommand = "gemini";
        public helpText = "Switch to the Gemini (graphical) user interface preserving " +
        "the current context.";
        protected minArguments = 0;
        protected maxArguments = 0;

        public isAvailableInCurrentContext(): boolean {
            return true;
        }
        doExecute(args: string, chained: boolean): void {
            const newPath = "/gemini/" + this.nglocation.path().split("/")[2];
            this.nglocation.path(newPath);
        };
    }
    export class Goto extends Command {

        public fullCommand = "goto";
        public helpText = "Go to the object referenced in a property, " +
        "or to a collection within an object, or an object within an open list or collection. " +
        "Goto takes one argument.  In the context of an object, that is the name or partial name" +
        "of the property or collection. In the context of an open list or collection, it is the " +
        "number of the item within the list or collection (starting at 1). ";
        protected minArguments = 1;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return this.isObject() || this.isList();
        }

        doExecute(args: string, chained: boolean): void {
            const arg0 = this.argumentAsString(args, 0);
            if (this.isList()) {
                const itemNo: number = parseInt(arg0);
                if (isNaN(itemNo)) {
                    this.clearInputAndSetMessage(arg0 + " is not a valid number");
                    return;
                }
                this.getList().then((list: ListRepresentation) => {
                    if (itemNo < 1 || itemNo > list.value().length) {
                        this.clearInputAndSetMessage(arg0 + " is out of range for displayed items");
                        return;
                    }
                    const link = list.value()[itemNo - 1]; // On UI, first item is '1'
                    this.urlManager.setItem(link, 1);
                });
                return;
            }
            if (this.isObject) {
                this.getObject()
                    .then((obj: DomainObjectRepresentation) => {
                        if (this.isCollection()) {
                            const item = this.argumentAsNumber(args, 0);
                            //TODO: validate range
                            const openCollIds = openCollectionIds(this.routeData());
                            const coll = obj.collectionMember(openCollIds[0]);
                            const link = coll.value()[item - 1];
                            this.urlManager.setItem(link, 1);
                            return;
                        } else {
                            const matchingProps = this.matchingProperties(obj, arg0);
                            const matchingRefProps = _.filter(matchingProps, (p) => { return !p.isScalar() });
                            const matchingColls = this.matchingCollections(obj, arg0);
                            var s: string = "";
                            switch (matchingRefProps.length + matchingColls.length) {
                                case 0:
                                    s = arg0 + " does not match any reference fields or collections";
                                    break;
                                case 1:
                                    //TODO: Check for any empty reference
                                    if (matchingRefProps.length > 0) {
                                        let link = matchingRefProps[0].value().link();
                                        this.urlManager.setItem(link, 1);
                                    } else { //Must be collection
                                        this.openCollection(matchingColls[0]);
                                    }
                                    break;
                                default:
                                    const props = _.reduce(matchingRefProps, (s, prop) => {
                                        return s + prop.extensions().friendlyName() + "\n";
                                    }, "");
                                    const colls = _.reduce(matchingColls, (s, coll) => {
                                        return s + coll.extensions().friendlyName() + "\n";
                                    }, "");
                                    s = "Multiple matches for " + arg0 + ":\n" + props + colls;
                            }
                            this.clearInputAndSetMessage(s);
                        }
                    });
            }
        };

        private openCollection(collection: CollectionMember): void {
            this.closeAnyOpenCollections();
            this.vm.clearInput();
            this.urlManager.setCollectionMemberState(1, collection.collectionId(), CollectionViewState.List);
        }
    }
    export class Help extends Command {

        public fullCommand = "help";
        public helpText = "If no argument specified, help lists the commands available in the current context." +
        "If help is followed by another command word as an argument (or an abbreviation of it), a description of that " +
        "specified Command will be returned.";
        protected minArguments = 0;
        protected maxArguments = 1;

        public isAvailableInCurrentContext(): boolean {
            return true;
        }

        doExecute(args: string, chained: boolean): void {
            var arg = this.argumentAsString(args, 0);
            if (arg) {
                try {
                    const c = this.commandFactory.getCommand(arg);
                    this.clearInputAndSetMessage(c.fullCommand + " command:\n" + c.helpText);
                } catch (Error) {
                    this.clearInputAndSetMessage(Error.message);
                }
            } else {
                const commands = this.commandFactory.allCommandsForCurrentContext();
                this.clearInputAndSetMessage(commands);
            }
        };
    }
    export class Menu extends Command {

        public fullCommand = "menu";
        public helpText = "Open a named main menu, from any context. " +
        "Menu takes one optional argument: the name, or partial name, of the menu. " +
        "If the partial name matches more than one menu, a list of matches is returned " +
        "but no menu is opened; if no argument is provided a list of all the menus " +
        "is returned.";
        protected minArguments = 0;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return true;
        }

        doExecute(args: string, chained: boolean): void {
            const name = this.argumentAsString(args, 0);
            this.context.getMenus()
                .then((menus: MenusRepresentation) => {
                    var links = menus.value();
                    if (name) {
                        //TODO: do multi-clause match
                        const exactMatches = _.filter(links, (t) => { return t.title().toLowerCase() === name; });
                        const partialMatches = _.filter(links, (t) => { return t.title().toLowerCase().indexOf(name) > -1; });
                        links = exactMatches.length === 1 ? exactMatches : partialMatches;
                    }
                    switch (links.length) {
                        case 0:
                            this.clearInputAndSetMessage(name + " does not match any menu");
                            break;
                        case 1:
                            const menuId = links[0].rel().parms[0].value;
                            this.urlManager.setHome(1);
                            this.urlManager.clearUrlState(1);
                            this.urlManager.setMenu(menuId, 1);
                            break;
                        default:
                            var label = name ? "Matching menus:\n" : "Menus:\n";
                            var s = _.reduce(links, (s, t) => { return s + t.title() + "\n"; }, label);
                            this.clearInputAndSetMessage(s);
                    }
                });
        }
    }
    export class OK extends Command {

        public fullCommand = "ok";
        public helpText = "Invoke the action currently open as a dialog. " +
        "Fields in the dialog should be completed before this.";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isDialog();
        }

        doExecute(args: string, chained: boolean): void {
            let fieldMap = this.routeData().dialogFields;
            this.getActionForCurrentDialog().then((action: ActionMember) => {

                if (chained && action.invokeLink().method() != "GET") {
                    this.mayNotBeChained(" unless the action is query-only");
                    return;
                }
                this.context.invokeAction(action, 1, fieldMap)
                    .then((err: ErrorMap) => {
                        if (err.containsError()) {
                            const paramFriendlyName = (paramId: string) => Helpers.friendlyNameForParam(action, paramId);
                            this.handleErrorResponse(err, paramFriendlyName);
                        } else {
                            this.urlManager.closeDialog(1);
                        }
                    });
            });
        };
    }
    export class Page extends Command {
        public fullCommand = "page";
        public helpText = "Supports paging of returned lists." +
        "The page command takes a single argument, which may be one of these four words: " +
        "first, previous, next, or last, which may be abbreviated down to the one character. " +
        "Alternative, the argument may be a specific page number.";
        protected minArguments = 1;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return this.isList();
        }

        doExecute(args: string, chained: boolean): void {
            const arg = this.argumentAsString(args, 0);
            this.getList().then((listRep: ListRepresentation) => {
                const numPages = listRep.pagination().numPages;
                const page = this.routeData().page;
                const pageSize = this.routeData().pageSize;
                if ("first".indexOf(arg) === 0) {
                    this.setPage(1);
                    return;
                } else if ("previous".indexOf(arg) === 0) {
                    if (page === 1) {
                        this.clearInputAndSetMessage("List is already showing the first page");
                    } else {
                        this.setPage(page - 1);
                    }
                } else if ("next".indexOf(arg) === 0) {
                    if (page === numPages) {
                        this.clearInputAndSetMessage("List is already showing the last page");
                    } else {
                        this.setPage(page + 1);
                    }
                } else if ("last".indexOf(arg) === 0) {
                    this.setPage(numPages);
                } else {
                    const number = parseInt(arg);
                    if (isNaN(number)) {
                        this.clearInputAndSetMessage("The argument must match: first, previous, next, last, or a single number");
                        return;
                    }
                    if (number < 1 || number > numPages) {
                        this.clearInputAndSetMessage("Specified page number must be between 1 and " + numPages);
                        return;
                    }
                    this.setPage(number);
                }
            });
        }

        private setPage(page) {
            const pageSize = this.routeData().pageSize;
            this.urlManager.setListPaging(1, page, pageSize, CollectionViewState.List);
        }
    }
    export class Property extends Command {

        public fullCommand = "property";
        public helpText = "Display the name and content of one or more properties of an object." +
        "Field may take 1 argument:  the partial field name. " +
        "If this matches more than one property, a list of matches is returned. " +
        "If no argument is provided, the full list of properties is returned. ";
        protected minArguments = 0;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return this.isObject();
        }

        doExecute(args: string, chained: boolean): void {
            const fieldName = this.argumentAsString(args, 0);
            this.getObject()
                .then((obj: DomainObjectRepresentation) => {
                    let props = this.matchingProperties(obj, fieldName);
                    let colls = this.matchingCollections(obj, fieldName);  //TODO -  include these
                    var s: string = "";
                    switch (props.length + colls.length) {
                        case 0:
                            if (!fieldName) {
                                s = "No visible properties";
                            } else {
                                s = fieldName + " does not match any properties";
                            }
                            break;
                        case 1:
                            if (props.length > 0) {
                                s = this.renderProp(props[0]);
                            } else {
                                s = this.renderColl(colls[0]);
                            }
                            break;
                        default:
                            s = _.reduce(props, (s, prop) => {
                                return s + this.renderProp(prop);
                            }, "");
                            s += _.reduce(colls, (s, coll) => {
                                return s + this.renderColl(coll);
                            }, "");
                    }
                    this.clearInputAndSetMessage(s);
                });
        }

        private renderProp(pm: PropertyMember): string {
            const name = pm.extensions().friendlyName();
            let value: string;
            const propInUrl = this.routeData().props[pm.propertyId()];
            if (this.isEdit() && !pm.disabledReason() && propInUrl) {
                value = propInUrl.toString() + " (modified)";
            } else {
                value = pm.value().toString() || "empty";
            }
            return name + ": " + value + "\n";
        }

        private renderColl(coll: CollectionMember): string {
            let output = coll.extensions().friendlyName() + " (collection): ";
            switch (coll.size()) {
                case 0:
                    output += "empty";
                    break;
                case 1:
                    output += "1 item";
                    break;
                default:
                    output += `${coll.size() } items`;
            }
            return output + "\n";
        }
    }
    export class Reload extends Command {

        public fullCommand = "reload";
        public helpText = "Not yet implemented. Reload the data from the server for an object or a list. " +
        "Note that for a list, which was generated by an action, reload runs the action again - " +
        "thus ensuring that the list is up to date. However, reloading a list does not reload the " +
        "individual objects in that list, which may still be cached. Invoking Reload on an " +
        "individual object, however, will ensure that its fields show the latest server data."
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isObject() || this.isList();
        }

        doExecute(args: string, chained: boolean): void {
            this.clearInputAndSetMessage("Reload command is not yet implemented");
        };
    }
    export class Root extends Command {

        public fullCommand = "root";
        public helpText = "From within an opend collection context, the root command returns" +
        " to the root object that owns the collection. Does not take any arguments";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isCollection();
        }

        doExecute(args: string, chained: boolean): void {
            this.closeAnyOpenCollections();
        };
    }
    export class Save extends Command {

        public fullCommand = "save";
        public helpText = "Not yet implemented. Save the updated fields on an object that is being edited, and return " +
        "from edit mode to a normal view of that object";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return this.isEdit() || this.isTransient();
        }
        doExecute(args: string, chained: boolean): void {
            if (chained) {
                this.mayNotBeChained();
                return;
            }
            this.getObject().then((obj: DomainObjectRepresentation) => {
                const props = obj.propertyMembers();
                const newValsFromUrl = this.routeData().props;
                const propIds = new Array<string>();
                const values = new Array<Value>();
                _.forEach(props, (propMember, propId) => {
                    if (!propMember.disabledReason()) {
                        propIds.push(propId);
                        const newVal = newValsFromUrl[propId];
                        if (newVal) {
                            values.push(newVal);
                        } else if (propMember.value().isNull() &&
                            propMember.isScalar()) {
                            values.push(new Value(""));
                        } else {
                            values.push(propMember.value());
                        }
                    }
                });
                const propMap = _.zipObject(propIds, values) as _.Dictionary<Value>;
                if (obj.extensions().renderInEdit()) { //i.e. it is a transient or a viewmodel
                    this.context.saveObject(obj, propMap, 1, true).then((err: ErrorMap) => this.handleError(err, obj));
                } else { //It is a persistent object being updated
                    this.context.updateObject(obj, propMap, 1, true).then((err: ErrorMap) => this.handleError(err, obj));
                }
            });
        };

        private handleError(err: ErrorMap, obj: DomainObjectRepresentation) {
            if (err.containsError()) {
                const propFriendlyName = (propId: string) => Helpers.friendlyNameForProperty(obj, propId);
                this.handleErrorResponse(err, propFriendlyName);
            } else {
                this.urlManager.setObjectEdit(false, 1);
            }
        }
    }
    export class Selection extends Command {

        public fullCommand = "selection";
        public helpText = "Not yet implemented. Select one or more items from" +
        "a list, prior to invoking an action on the selection." +
        "Selection has one mandatory argument, which must be one of these words, " +
        "though it may be abbreviated: add, remove, all, clear, show. " +
        "The Add and Remove options must be followed by a second argument specifying " +
        "the item number, or range, to be added or removed.";
        protected minArguments = 1;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return this.isList();
        }
        doExecute(args: string, chained: boolean): void {
            //TODO: Add in sub-commands: Add, Remove, All, Clear & Show
            const arg = this.argumentAsString(args, 0);
            const {start, end} = this.parseRange(arg); //'destructuring'
            this.getList().then((list: ListRepresentation) => {
                this.selectItems(list, start, end);
            });
        };

        private selectItems(list: ListRepresentation, startNo: number, endNo: number): void {
            let itemNo: number;
            for (itemNo = startNo; itemNo <= endNo; itemNo++) {
                this.urlManager.setListItem(1, itemNo - 1, true);
            }
        }
    }
    export class Show extends Command {

        public fullCommand = "show";
        public helpText = "Show one or more of the items from or a list view, or " +
        "an opened object collection. If no arguments are specified, show will list all of the " +
        "the items in the opened object collection, or the first page of items if in a list view. " +
        "Alternatively, the command may be specified with an item number, or a range such as 3-5. " +
        "Not yet implemented: Show can take additional parameters to specify table view, and/or to " +
        "specify logical matches for items to be shown e.g. status='pending'"
        protected minArguments = 0;
        protected maxArguments = 1;

        isAvailableInCurrentContext(): boolean {
            return this.isCollection() || this.isList();
        }

        doExecute(args: string, chained: boolean): void {
            let arg = this.argumentAsString(args, 0, true);
            const {start, end} = this.parseRange(arg);
            if (this.isCollection()) {
                this.getObject().then((obj: DomainObjectRepresentation) => {
                    const openCollIds = openCollectionIds(this.routeData());
                    const coll = obj.collectionMember(openCollIds[0]);
                    this.renderItems(coll, start, end);
                });
                return;
            }
            //must be List
            this.getList().then((list: ListRepresentation) => {
                this.renderItems(list, start, end);
            });
        };

        private renderItems(source: IHasLinksAsValue, startNo: number, endNo: number): void {
            const max = source.value().length;
            if (!startNo) {
                startNo = 1;
            }
            if (!endNo) {
                endNo = max;
            }
            if (startNo > max || endNo > max) {
                this.clearInputAndSetMessage("The highest numbered item is " + source.value().length);
                return;
            }
            if (startNo > endNo) {
                this.clearInputAndSetMessage("Starting item number cannot be greater than the ending item number");
                return;
            }
            let output = "";
            let i: number;
            const links = source.value();
            for (i = startNo; i <= endNo; i++) {
                output += "Item " + i + ": " + links[i - 1].title() + "\n";
            }
            this.clearInputAndSetMessage(output);
        }
    }
    export class Where extends Command {

        public fullCommand = "where";
        public helpText = "Display a reminder of the current context.";
        protected minArguments = 0;
        protected maxArguments = 0;

        isAvailableInCurrentContext(): boolean {
            return true;
        }

        doExecute(args: string, chained: boolean): void {
            this.$route.reload();
        };
    }
}