grammar AzSQL;

stat:
		select_clause
	;

select_clause:
		K_SELECT column_list
		K_FROM table_name
		(where_clause)?
		;

column_list:
		  '*'
		| column_name (COMMA column_name)* 
		;

column_name:
		ID
		;

table_name:
	ID
	;

where_clause:
		K_WHERE expr
		;

expr
 : 
   K_NOT expr                                                                   # notExpr
 | ID  COMPARISON_OP NEGATION? literal                                          # baseComparison
 | expr K_AND expr                                                              # andExpr
 | expr K_OR expr                                                               # orExpr
 | '(' expr ')'                                                                 # paranthesizedExpr
 ;
		

literal:
		STRING_LITERAL | NUMERIC_LITERAL | BOOLEAN_LITERAL
	;
	
COMPARISON_OP:
        '<' | '<=' | '>' | '>=' | '=' | '==' | '!=' | '<>'
		;

STRING_LITERAL
 : '\'' ( ~'\'' | '\'\'' )* '\''
 ;

NUMERIC_LITERAL
 : DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )?
 | '.' DIGIT+ ( E [-+]? DIGIT+ )?
 ; 
	
BOOLEAN_LITERAL:
		TRUE | FALSE;
	
K_SELECT : S E L E C T;
K_WHERE : W H E R E;
K_FROM : F R O M;
K_AND: A N D;
K_OR: O R;
K_NOT: N O T;

TRUE: T R U E;
FALSE: F A L S E;
COMMA: ',';

ORDER: 'order';
GROUP: 'group';
BY: 'by';

ID: ('a'..'z' | 'A' .. 'Z') ('a'..'z' | 'A' .. 'Z' | '0'..'9' | '_')* ;

NEGATION: '-';

SINGLE_LINE_COMMENT
 : '--' ~[\r\n]* -> channel(HIDDEN)
 ;

MULTILINE_COMMENT
 : '/*' .*? ( '*/' | EOF ) -> channel(HIDDEN)
 ;

SPACES
 : [ \u000B\t\r\n] -> channel(HIDDEN)
 ;


fragment DIGIT : [0-9];

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];