export class Process {
  constructor(private _generator: GeneratorFunction) {

  }

  start() {
    return new ActiveProcess(this._generator());
  }

  *[Symbol.iterator]() {
    return this._generator();
  }
}

export enum ProcessStatus {
  ACTIVE,
  FINISHED
}

export class ActiveProcess {
  public _status: ProcessStatus;

  constructor(private _generator: Generator<any, any, any>) {
    this._status = ProcessStatus.ACTIVE;
  }

  next(value: any) {
    const result = this._generator.next(value);
    if (result.done) {
      this._status = ProcessStatus.FINISHED;
    }
    return result;
  }

  *[Symbol.iterator]() {
    yield* this._generator;
    this._status = ProcessStatus.FINISHED;
  }
}

export function start(a: Process) {
  return a.start();
}

export function resume_with(a: ActiveProcess, value: any, yield_fun: Function, done_Fun: Function) {
  const result = a.next(value);
  if (result.done) {
    return done_Fun(result.value);
  } else {
    return yield_fun(result.value);
  }
}