import { BehaviorSubject } from "rxjs";
import { BaseTransition, StateMachineState } from "./statemachine-state.model";

export class EventCondition extends BaseTransition {
  static TYPE: string = "event";
  private _eventRef: string = "";
  private label: BehaviorSubject<string> | undefined;
  get eventRef() {
    return this._eventRef;
  }
  set eventRef(str: string) {
    this._eventRef = str;
    this.label?.next(this._eventRef);
  }

  constructor() {
    super();
    this.label = new BehaviorSubject<string>("");
  }

  public override getType(): string {
    return EventCondition.TYPE;
  }

  public override getLabel(): BehaviorSubject<string> | undefined {
    return this.label;
  }
}

export class DataCondition extends BaseTransition {
  static TYPE: string = "data";
  private _condition: string = "";
  name: string | undefined;
  get condition() : string {
    return this._condition;
  }
  set condition(str: string) {
    this._condition = str;
    this.label?.next(this._condition);
  }
  private label: BehaviorSubject<string> | undefined;

  constructor() {
    super();
    this.label = new BehaviorSubject<string>("");
  }

  public override getType(): string {
    return DataCondition.TYPE;
  }

  public override getLabel(): BehaviorSubject<string> | undefined {
    return this.label;
  }
}

export class SwitchStateMachineState extends StateMachineState {
  public static TYPE: string = "switch";

  constructor() {
    super();
    this.type = SwitchStateMachineState.TYPE;
    this.eventConditions = [];
    this.dataConditions = [];
  }

  eventConditions: EventCondition[];
  dataConditions: DataCondition[] = [];
  defaultCondition: BaseTransition | null = null;

  public override setTransitions(transitions: BaseTransition[]) {
    if (transitions) {
      this.eventConditions = transitions.filter((t: BaseTransition) => {
        return t.getType() == EventCondition.TYPE;
      }).map((t: BaseTransition) => {
        return t as EventCondition;
      });
      this.dataConditions = transitions.filter((t: BaseTransition) => {
        return t.getType() == DataCondition.TYPE;
      }).map((t: BaseTransition) => {
        return t as DataCondition;
      });
    }

    return [];
  }

  public override tryAddTransition(transitionName: string): string | null {
    const newEvtTransition = new EventCondition();
    newEvtTransition.eventRef = "event";
    newEvtTransition.transition = transitionName;
    this.eventConditions.push(newEvtTransition);
    return null;
  }

  public override getNextTransitions(): BaseTransition[] {
    let arr: BaseTransition[] = [];
    if (this.defaultCondition && this.defaultCondition.transition) {
      arr.push(this.defaultCondition);
    }

    this.eventConditions.forEach((ec) => {
      arr.push(ec);
    });

    this.dataConditions.forEach((dc) => {
      arr.push(dc);
    });

    return arr;
  }
}

export class DefaultCondition {
  transition: string | undefined;
}
