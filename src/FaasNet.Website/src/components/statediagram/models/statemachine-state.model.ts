import { BehaviorSubject } from "rxjs";

export abstract class StateMachineState {
  id: string | undefined;
  name: string | undefined;
  type: string | undefined;

  public abstract setTransitions(transitions: BaseTransition[]): void;
  public abstract tryAddTransition(transitionName: string): string | null;
  public abstract getNextTransitions(): BaseTransition[];
}

export abstract class BaseTransition {
  transition: string = "";
  public abstract getType(): string;
  public abstract getLabel(): BehaviorSubject<string> | undefined;
}

export class EmptyTransition extends BaseTransition {
  static TYPE: string = "empty";
  private label: BehaviorSubject<string> | undefined;

  constructor() {
    super();
    this.label = new BehaviorSubject<string>("transition");
  }

  public override getType(): string {
    return EmptyTransition.TYPE;
  }

  public override getLabel(): BehaviorSubject<string> | undefined {
    return this.label;
  }
}

export class FlowableStateMachineState extends StateMachineState{
  constructor() {
    super();
    this.transition = "";
  }

  transition: string;

  public override setTransitions(transitions: BaseTransition[]): void {
    if (transitions && transitions.length > 0) {
      this.transition = transitions[0].transition;
    }
  }

  public override tryAddTransition(transitionName: string): string | null{
    const oldTransition = this.transition;
    this.transition = transitionName;
    return oldTransition;
  }

  public override getNextTransitions(): BaseTransition[] {
    const record: EmptyTransition = new EmptyTransition();
    record.transition = this.transition;
    return [
      record
    ];
  }
}
