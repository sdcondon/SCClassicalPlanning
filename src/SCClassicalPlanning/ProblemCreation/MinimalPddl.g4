/*
 * Absolutely minimal PDDL (1.2) grammar, based on the spec retrieved (on 12th March 2023) from:
 * https://homepages.inf.ed.ac.uk/mfourman/tools/propplan/pddl.pdf.
 * Absolutely NO extensions (not even typing) are recognised - STRIPS only.
 * Have also omitted problem situations and length specs. While they aren't marked as a particular
 * extension by the spec, they are marked as deprecated by this (well, length spec isn't even mentioned):
 * https://planning.wiki/ref/pddl/problem. That may just be due to incompleteness of this wiki, but
 * I'm taking every excuse I can to minimise this.
 *
 */
grammar MinimalPDDL;

singleDomain: domain EOF;
singleProblem: problem EOF;

////////// DOMAINS

domain: '(' 'define' '(' 'domain' NAME ')' extendsDef? requirementsDef? constantsDef? predicatesDef? timelessDef? structureDef* ')';

extendsDef:      '(' ':extends' NAME+ ')';
requirementsDef: '(' ':requirements' REQUIREMENT_FLAG+ ')';
constantsDef:    '(' ':constants' constantDecList ')';
predicatesDef:   '(' ':predicates' (predicates+=predicateDef)+ ')';
timelessDef:     '(' ':timeless' literalList ')'; // grammar doesn't specify that it has to be ground, perhaps should..
structureDef:    '(' ':action' NAME ':parameters' '(' variableDecList ')' (':precondition' goal)? (':effect' effect)? ')' # ActionDef
            ;

effect: literal                      # LiteralEffectElement
      | '(' 'and' effect effect+ ')' # ConjunctionEffectElement                     
      ;



////////// PROBLEMS

problem: '(' 'define' '(' 'problem' NAME ')' '(' ':domain' NAME ')' requirementsDef? objectsDef? initDef? goalDef+ ')';

objectsDef: '(' ':objects' constantDecList ')';
initDef:    '(' ':init' literalList ')'; // grammar doesn't specify that it has to be ground, perhaps should..
goalDef:    '(' ':goal' goal ')';



////////// GOALS

goal: literal                  # LiteralGoalElement
    | '(' 'and' goal goal+ ')' # ConjunctiveGoalElement
    ;



////////// LITERALS, PREDICATES & TERMS

literalList: (elements+=literal)+;
literal: predicate               # PositiveLiteral
       | '(' 'not' predicate ')' # NegativeLiteral
       ;

predicateDef: '(' NAME variableDecList ')';
predicate: '(' NAME termList ')';

termList: (elements+=term)*;
term: NAME          # ConstantTerm
    | VARIABLE_NAME # VariableTerm
    ;

// NB: this is where we'd add typing support:
variableDecList: (elements+=VARIABLE_NAME)*;
constantDecList: (elements+=NAME)*;



////////// LEXER

REQUIREMENT_FLAG: ':' NAME;
VARIABLE_NAME: '?' NAME;
// Too restrictive?
NAME: [-a-zA-Z]+;

WHITESPACE: [ \t\r\n]+ -> skip;
// Also need to skip comments..
