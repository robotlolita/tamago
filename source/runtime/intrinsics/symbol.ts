export function make(description: string) {
  return Symbol(description);
}

export function get_description(a: symbol) {
  return a.description;
}